CREATE PROCEDURE famille_accueil_insertion(
    OUT p_fa_id INT,
    IN p_date_debut DATE,
    IN p_date_fin DATE,
    IN p_identifiant_animal VARCHAR,
    IN p_identifiant_contact INT
)
    LANGUAGE plpgsql
AS
$$
DECLARE
    existe_animal   animal%ROWTYPE;
    existe_contact  contact%ROWTYPE;
    fa_active       famille_accueil%ROWTYPE;
    adoption_active adoption%ROWTYPE;
BEGIN
    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : animal %s introuvable.',
                p_identifiant_animal
                                        );
    END IF;

    -- Vérification contact
    SELECT *
    INTO existe_contact
    FROM contact
    WHERE contact_identifiant = p_identifiant_contact;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut pas être placé en famille d''accueil
    IF existe_animal.date_deces IS NOT NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : animal %s est décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Contrainte : la date de début peut être au maximum 5 jours dans le futur
    -- afin de permettre un encodage légèrement anticipé tout en restant réaliste
    IF p_date_debut > CURRENT_DATE + INTERVAL '5 days' THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : la date de début %s ne peut pas dépasser 5 jours dans le futur.',
                p_date_debut
                                        );
    END IF;

    -- Contrainte : la date de fin doit être supérieure ou égale à la date de début (contrainte table)
    IF p_date_fin IS NOT NULL AND p_date_fin < p_date_debut THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : la date de fin %s ne peut pas être inférieure à la date de début %s.',
                p_date_fin,
                p_date_debut
                                        );
    END IF;

    -- Contrainte : un animal ne peut pas avoir plusieurs accueils actifs simultanément
    -- un seul accueil avec date_fin IS NULL à la fois
    SELECT *
    INTO fa_active
    FROM famille_accueil
    WHERE fa_ani_identifiant = p_identifiant_animal
      AND date_fin IS NULL
    ORDER BY date_debut DESC
    LIMIT 1;

    IF FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : animal %s est déjà en famille d''accueil sans date de retour fixée.',
                p_identifiant_animal
                                        );
    END IF;

    -- Contrainte : un animal ayant une adoption avec statut demande ou acceptee
    -- ne peut pas être placé en famille d''accueil
    SELECT *
    INTO adoption_active
    FROM adoption
    WHERE ani_identifiant = p_identifiant_animal
      AND statut IN ('demande', 'acceptee')
    ORDER BY date_demande DESC
    LIMIT 1;

    IF FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion famille accueil impossible : animal %s a une adoption en cours avec le statut %s.',
                p_identifiant_animal,
                adoption_active.statut
                                        );
    END IF;

    -- Insertion
    INSERT INTO famille_accueil (date_debut,
                                 date_fin,
                                 fa_ani_identifiant,
                                 fa_contact)
    VALUES (p_date_debut,
            p_date_fin,
            p_identifiant_animal,
            p_identifiant_contact)
    RETURNING fa_id INTO p_fa_id; -- renvoie l''id généré par le SERIAL

END;
$$;


CREATE PROCEDURE famille_accueil_modification(
    IN p_fa_id INT,
    IN p_date_debut DATE,
    IN p_date_fin DATE,
    IN p_identifiant_animal VARCHAR,
    IN p_identifiant_contact INT
)
    LANGUAGE plpgsql
AS
$$
DECLARE
    existe         famille_accueil%ROWTYPE;
    existe_animal  animal%ROWTYPE;
    existe_contact contact%ROWTYPE;
BEGIN
    -- Vérification existence famille accueil
    SELECT *
    INTO existe
    FROM famille_accueil
    WHERE fa_id = p_fa_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification impossible : famille accueil %s introuvable.',
                p_fa_id
                                        );
    END IF;

    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification famille accueil impossible : animal %s introuvable.',
                p_identifiant_animal
                                        );
    END IF;

    -- Vérification contact
    SELECT *
    INTO existe_contact
    FROM contact
    WHERE contact_identifiant = p_identifiant_contact;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification famille accueil impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut pas avoir de modification de famille accueil
    IF existe_animal.date_deces IS NOT NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification famille accueil impossible : animal %s est décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Contrainte : la date de début peut être au maximum 5 jours dans le futur
    -- afin de permettre un encodage légèrement anticipé tout en restant réaliste
    IF p_date_debut > CURRENT_DATE + INTERVAL '5 days' THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification famille accueil impossible : la date de début %s ne peut pas dépasser 5 jours dans le futur.',
                p_date_debut
                                        );
    END IF;

    -- Contrainte : la date de fin doit être supérieure ou égale à la date de début (contrainte table)
    IF p_date_fin IS NOT NULL AND p_date_fin < p_date_debut THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Modification famille accueil impossible : la date de fin %s ne peut pas être inférieure à la date de début %s.',
                p_date_fin,
                p_date_debut
                                        );
    END IF;

    -- Contrainte : si le statut passe de actif (date_fin IS NULL) à terminé (date_fin IS NOT NULL)
    -- une entrée avec raison retour_famille_accueil sera insérée par le code C#
    -- On vérifie donc que la date de fin n''est pas antérieure à la dernière sortie de l''animal
    IF existe.date_fin IS NULL AND p_date_fin IS NOT NULL THEN
        IF NOT EXISTS (SELECT 1
                       FROM ani_sortie
                       WHERE ani_identifiant = p_identifiant_animal
                         AND raison = 'famille_accueil'
                         AND date_sortie <= p_date_fin) THEN
            RAISE EXCEPTION USING MESSAGE = FORMAT(
                    'Modification famille accueil impossible : aucune sortie famille accueil trouvée pour l''animal %s.',
                    p_identifiant_animal
                                            );
        END IF;
    END IF;

    -- Modification
    UPDATE famille_accueil
    SET date_debut         = p_date_debut,
        date_fin           = p_date_fin,
        fa_ani_identifiant = p_identifiant_animal,
        fa_contact         = p_identifiant_contact
    WHERE fa_id = p_fa_id;

END;
$$;


CREATE PROCEDURE famille_accueil_suppression(
    IN p_fa_id INT
)
    LANGUAGE plpgsql
AS
$$
DECLARE
    existe famille_accueil%ROWTYPE;
BEGIN
    -- Vérification existence
    SELECT *
    INTO existe
    FROM famille_accueil
    WHERE fa_id = p_fa_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Suppression impossible : famille accueil %s introuvable.',
                p_fa_id
                                        );
    END IF;

    -- Contrainte : une famille accueil active (sans date de fin) ne peut pas être supprimée
    -- car elle a généré une sortie dans ani_sortie. Il faut d''abord enregistrer
    -- le retour de l''animal via ani_entree avec raison retour_famille_accueil.
    IF existe.date_fin IS NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Suppression impossible : la famille accueil %s est encore active. Enregistrez d''abord le retour de l''animal.',
                p_fa_id
                                        );
    END IF;

    -- Suppression
    DELETE
    FROM famille_accueil
    WHERE fa_id = p_fa_id;

END;
$$;


CREATE FUNCTION famille_accueil_afficher_liste_complete()
    RETURNS TABLE
            (
                fa_id              INT,
                date_debut         DATE,
                date_fin           DATE,
                fa_ani_identifiant VARCHAR,
                fa_contact         INT
            )
    LANGUAGE plpgsql
AS
$$
BEGIN

    RETURN QUERY
        SELECT fa.fa_id,
               fa.date_debut,
               fa.date_fin,
               fa.fa_ani_identifiant,
               fa.fa_contact
        FROM famille_accueil fa;

END;
$$;


CREATE FUNCTION famille_accueil_afficher_un(p_fa_id INT)
    RETURNS TABLE
            (
                fa_id           INT,
                date_debut      DATE,
                date_fin        DATE,
                ani_identifiant VARCHAR,
                ani_nom         VARCHAR,
                fa_contact      INT,
                contact_nom     VARCHAR,
                contact_prenom  VARCHAR
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    existe famille_accueil%ROWTYPE;
BEGIN
    -- Vérifier que la famille accueil existe
    SELECT *
    INTO existe
    FROM famille_accueil
    WHERE fa_id = p_fa_id;

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'La famille accueil % n''est pas présente dans la liste.',
            p_fa_id;
    END IF;

    -- Retourner la famille accueil avec les informations de l''animal et du contact
    -- La jointure permet d''éviter plusieurs appels depuis le C#
    RETURN QUERY
        SELECT fa.fa_id,
               fa.date_debut,
               fa.date_fin,
               fa.fa_ani_identifiant AS ani_identifiant,
               a.nom                 AS ani_nom,
               fa.fa_contact,
               c.nom                 AS contact_nom,
               c.prenom              AS contact_prenom
        FROM famille_accueil fa
                 JOIN animal a ON a.identifiant = fa.fa_ani_identifiant
                 JOIN contact c ON c.contact_identifiant = fa.fa_contact
        WHERE fa.fa_id = p_fa_id;

END;
$$;


CREATE FUNCTION famille_accueil_afficher_par_animal(p_identifiant_animal VARCHAR)
    RETURNS TABLE
            (
                fa_id           INT,
                date_debut      DATE,
                date_fin        DATE,
                ani_identifiant VARCHAR,
                ani_nom         VARCHAR,
                fa_contact      INT,
                contact_nom     VARCHAR,
                contact_prenom  VARCHAR
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    existe_animal animal%ROWTYPE;
BEGIN
    -- Vérifier que l''animal existe
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'Animal % introuvable.',
            p_identifiant_animal;
    END IF;

    -- Retourner toutes les familles d''accueil liées à cet animal avec les informations du contact
    -- La left join est pertinente ici car si l''animal n''a aucune famille d''accueil la jointure
    -- ne retournera rien alors que l''animal existe
    RETURN QUERY
        SELECT fa.fa_id,
               fa.date_debut,
               fa.date_fin,
               fa.fa_ani_identifiant AS ani_identifiant,
               a.nom                 AS ani_nom,
               fa.fa_contact,
               c.nom                 AS contact_nom,
               c.prenom              AS contact_prenom
        FROM famille_accueil fa
                 JOIN animal a ON a.identifiant = fa.fa_ani_identifiant
                 JOIN contact c ON c.contact_identifiant = fa.fa_contact
        WHERE fa.fa_ani_identifiant = p_identifiant_animal
        ORDER BY fa.date_debut DESC;

END;
$$;