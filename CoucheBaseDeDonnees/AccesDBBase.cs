using System.Data;
using Npgsql;

namespace Accardi_Alessandro_Refuge_WPF.CoucheBaseDeDonnees
{
    internal abstract class AccesDBBase<T>
    {
        //Propriétées-------------------------------------------------------------------------------------------------------------------------------------------------------------

        //Chaque DAO définit sa table, insert, update, delete et d'éventuels paramètres de sorti (attention pour ceux prévoir les nulls !!!!!!)
        protected abstract string AfficherListe { get; }
        protected abstract string? AfficherUnElement { get; }
        protected abstract string Insert { get; }
        protected abstract string? InsertOut { get; }
        protected abstract string Update { get; }
        protected abstract string? UpdateOut { get; }
        protected abstract string Delete { get; }
        protected abstract string? DeleteOut { get; }

        protected virtual string? ClesPrimaire => null;   // Nom du paramètre clé primaire — utilisé pour éviter le doublon lors d'un INSERT
        protected virtual bool DeleteUtiliseCleSeule => false;  // Indique si DeleteAsync doit appeler AssignerParametreSQL ou uniquement envoyer la clé primaire

        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //Méthodes ----------------------------------------------------------------------------------------------------------------------------------------------------------------

        //L'utilité de Task pour le système asynchrone et de "promettre" que le retour sera fourni, mais en attendant la lecture de la DB
        //l'application peut faire autre chose et ne pas être bloqué en attendant la fin de la lecture de la DB.
        //Raisons pour lesquelles la méthode ci-dessous contient trois using :
        //              - La connexion doit vivre du début à la fin             (using var connexion = ConnexionDB.GetConnexion()))
        //              - La commande doit vivre juste le temps de l'exécution  (using (var cmd = new NpgsqlCommand(sql, connexion)))
        //              - Le reader doit vivre juste le temps de la lecture     (using (var reader = await cmd.ExecuteReaderAsync()))

        protected abstract T ConvertirEnObjet(IDataReader reader);             //Mapping d'une ligne SQL (l'objet métier)
                                                                               //Dans cette méthode T sera remplacé par l'objet métier
                                                                               //IDataReader est l'objet qui me permet de parcourir les lignes.
                                                                               //La méthode Map va permettre de récupérer toute une ligne (un enregistrement) et la transformer en un objet
                                                                               //C# (récupération)
                                                                               //Quant à IDataReader il s'agit de l'objet qui contiendra le résultat de la requête SQL
                                                                               //(la ligne ne cours de lecture)

        protected abstract void AssignerParametreSQL(NpgsqlCommand cmd, T objet);    //A l'inverse de la méthode ci-dessus la méthode BindParameters permet de transformer
                                                                                     //un objet C# en valeurs pour les paramètres de la requête SQL (envoi)
                                                                                     //NpgsqlCommand cmd : C'est la commande SQL dans laquelle on va injecter les valeurs.

        protected virtual void AssignerCleSQL(NpgsqlCommand cmd, T objet) { }  // Méthode optionnelle à surcharger quand DeleteUtiliseCleSeule = true
                                                                               // Elle n'envoie que la clé primaire à la procédure de suppression
                                                                               // Par défaut elle ne fait rien — seules les classes qui ont DeleteUtiliseCleSeule = true
                                                                               // doivent la surcharger

        public virtual async Task<string?> InsertAsync(T objet)
        {
            NpgsqlParameter? retour = null;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                using (var cmd = new NpgsqlCommand(Insert, connexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;  // L'ajoute de la ligne: cmd.CommandType = CommandType.StoredProcedure;  est très important.
                                                                    //  Car elle permet de signaler que la commande qui va être exécutée est une procédure stockée ou une fonction
                                                                    // et non pas une commande SQL "classique". 

                    // Si la classe fille fournit un nom de paramètre OUT
                    if (!string.IsNullOrEmpty(InsertOut))
                    {
                        retour = new NpgsqlParameter(InsertOut, NpgsqlTypes.NpgsqlDbType.Varchar)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(retour);
                    }

                    AssignerParametreSQL(cmd, objet);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return retour?.Value?.ToString();
        }


        public virtual async Task<string?> UpdateAsync(T objet)
        {
            NpgsqlParameter? retour = null;   // Déclaré ici pour être accessible après les using

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                using (var cmd = new NpgsqlCommand(Update, connexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;  // L'ajoute de la ligne: cmd.CommandType = CommandType.StoredProcedure;  est très important.
                                                                    //  Car elle permet de signaler que la commande qui va être exécutée est une procédure stockée ou une fonction
                                                                    // et non pas une commande SQL "classique". 

                    //Si la classe fille fournit un nom de paramètre OUT
                    if (!string.IsNullOrEmpty(UpdateOut))
                    {
                        retour = new NpgsqlParameter(UpdateOut, NpgsqlTypes.NpgsqlDbType.Varchar)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(retour);
                    }

                    AssignerParametreSQL(cmd, objet);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            return retour?.Value?.ToString();
        }


        public virtual async Task<string?> DeleteAsync(T objet)
        {
            NpgsqlParameter? retour = null;   // Déclaré ici pour être accessible après les using

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                using (var cmd = new NpgsqlCommand(Delete, connexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;  // L'ajoute de la ligne: cmd.CommandType = CommandType.StoredProcedure;  est très important.
                                                                    //  Car elle permet de signaler que la commande qui va être exécutée est une procédure stockée ou une fonction
                                                                    // et non pas une commande SQL "classique". 

                    //Si la classe fille fournit un nom de paramètre OUT
                    if (!string.IsNullOrEmpty(DeleteOut))
                    {
                        retour = new NpgsqlParameter(DeleteOut, NpgsqlTypes.NpgsqlDbType.Varchar)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(retour);
                    }

                    // Si la classe fille indique que le DELETE n'utilise que la clé primaire
                    // on appelle AssignerCleSQL qui n'envoie que l'identifiant
                    // sinon on appelle AssignerParametreSQL qui envoie tous les paramètres
                    if (DeleteUtiliseCleSeule)
                        AssignerCleSQL(cmd, objet);
                    else
                        AssignerParametreSQL(cmd, objet);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return retour?.Value?.ToString();
        }

        protected async Task<List<T>> ExecuterFonctionRetourListeAsync(string nomFonction)
        {
            List<T> retVal = new List<T>();

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();
                string sql = $"SELECT * FROM {nomFonction}()";

                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.CommandType = CommandType.Text;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            retVal.Add(ConvertirEnObjet(reader));
                        }
                    }
                }
            }
            return retVal;
        }

        protected async Task<T?> ExecuterFonctionRetourObjetAsync(string nomFonction, string nomParametre, object valeurParametre)
        {
            T? retVal = default;

            using (var connexion = ConnexionDB.GetConnexion())
            {
                await connexion.OpenAsync();

                string sql = $"SELECT * FROM {nomFonction}(@{nomParametre})"; //remarque ne pas oublier d'indiquer le @ quand il s'agit d'un paramètre sans quoi ça ne fonctionne
                                                                              //pas terrible ....
                using (var cmd = new NpgsqlCommand(sql, connexion))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue($"@{nomParametre}", valeurParametre); //L'usage du $ ici est l'interpolation afin que la chaine soit remplacée par la valeur contenue dans la variable.

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            retVal = ConvertirEnObjet(reader);
                        }
                    }
                }
            }
            return retVal;
        }

        /********************************************************************************************************************************
         *                                      OUTILS DE MAPPING (UTILISABLE PAR LES DAO)
         ********************************************************************************************************************************/

        // Lecture sécurisée (null-safe)
        //Dans cette méthode les mots clefs TValue vont prendre le type de retour sélectionné par le développeur.
        //<TValue> : sera remplacé par le type de retour attendu par la variable de la classe C#
        protected TValue GetValueOrDefault<TValue>(IDataRecord reader, string nomColonne)
        {
            TValue resultat;
            int index = reader.GetOrdinal(nomColonne);

            if (reader.IsDBNull(index))
            {
                resultat = default(TValue);
            }
            else
            {
                object val = reader.GetValue(index);

                // Conversion DateOnly -> DateTime si nécessaire
                if (typeof(TValue) == typeof(DateTime) && val is DateOnly d)
                {
                    resultat = (TValue)(object)d.ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    resultat = (TValue)val;
                }
            }

            return resultat;
        }

        // Lecture string null-safe
        protected string GetStringSafe(IDataRecord reader, string nomColonne)
        {
            int index = reader.GetOrdinal(nomColonne);
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        // Lecture DateTime? null-safe
        protected DateTime? GetDateTimeSafe(IDataRecord reader, string nomColonne)
        {
            DateTime? resultat;
            int index = reader.GetOrdinal(nomColonne);

            if (reader.IsDBNull(index))
            {
                resultat = null;
            }
            else
            {
                object val = reader.GetValue(index);

                if (val is DateOnly d)
                {
                    resultat = d.ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    resultat = (DateTime)val;
                }
            }

            return resultat;
        }

        // Lecture bool null-safe
        protected bool? GetBoolSafe(IDataRecord reader, string nomColonne)
        {
            int index = reader.GetOrdinal(nomColonne);
            return reader.IsDBNull(index) ? (bool?)null : reader.GetBoolean(index);
        }
    }
}