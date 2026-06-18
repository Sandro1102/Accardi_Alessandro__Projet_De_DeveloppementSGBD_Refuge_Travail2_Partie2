create or replace procedure public.animal_insertion(OUT p_identifiant character varying, IN p_nom character varying, IN p_type character varying, IN p_sexe character, IN p_sterilise boolean, IN p_datenaissance date, IN p_datedeces date, IN p_datesterilisation date, IN p_particularites text, IN p_description text)
    language plpgsql
as
$$
DECLARE
    existe    animal%ROWTYPE;
    prefix    VARCHAR(6);
    dernierId VARCHAR(11);
    seq       INT;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier si l'animal existe déjà
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM animal a
    WHERE a.nom            = p_nom
      AND a.type           = p_type
      AND a.date_naissance = p_dateNaissance
      AND (
            (a.date_sterilisation IS NULL AND p_dateSterilisation IS NULL)
            OR
            (a.date_sterilisation = p_dateSterilisation)
          );
    -- Remarque : le sexe n'est pas inclus dans la détection de doublon car la méthode
    -- EstIdentiqueA en C# ne le vérifie pas non plus. Les deux endroits doivent
    -- appliquer la même règle.

    IF FOUND THEN
        RAISE EXCEPTION USING
            MESSAGE = 'L''animal que vous tentez d''insérer est déjà présent dans la liste du refuge',
            DETAIL  = FORMAT('Il possède l''identifiant suivant : %s', existe.identifiant);
    END IF;

    --------------------------------------------------------------------
    -- 2) Vérifications métier sur les longueurs
    --------------------------------------------------------------------
    IF p_nom IS NULL OR LENGTH(TRIM(p_nom)) = 0 THEN
        RAISE EXCEPTION 'Le nom ne peut pas être vide';
    END IF;

    IF LENGTH(p_nom) > 50 THEN
        RAISE EXCEPTION 'Le nom ne peut pas dépasser 50 caractères';
    END IF;

    IF p_nom !~ '^[A-Za-zÀ-ÖØ-öø-ÿ\- ]+$' THEN
        RAISE EXCEPTION 'Le nom ne peut contenir que des lettres, espaces et tirets';
    END IF;

    IF p_type IS NULL OR LENGTH(TRIM(p_type)) = 0 THEN
        RAISE EXCEPTION 'Le type ne peut pas être vide';
    END IF;

    IF LENGTH(p_type) > 10 THEN
        RAISE EXCEPTION 'Le type ne peut pas dépasser 10 caractères';
    END IF;

    IF p_type NOT IN ('chien', 'chat') THEN
        RAISE EXCEPTION 'Seuls les types "chien" ou "chat" sont acceptés';
    END IF;

    IF p_sexe NOT IN ('M', 'F') THEN
        RAISE EXCEPTION 'Le sexe doit être M ou F';
    END IF;

    IF p_particularites IS NOT NULL AND LENGTH(p_particularites) > 500 THEN
        RAISE EXCEPTION 'Les particularités ne peuvent pas dépasser 500 caractères';
    END IF;

    IF p_description IS NOT NULL AND LENGTH(p_description) > 2000 THEN
        RAISE EXCEPTION 'La description ne peut pas dépasser 2000 caractères';
    END IF;

    --------------------------------------------------------------------
    -- 3) Vérifications métier sur les dates
    --------------------------------------------------------------------
    -- La date de naissance ne peut pas être dans le futur (contrainte table)
    IF p_dateNaissance > CURRENT_DATE THEN
        RAISE EXCEPTION 'La date de naissance ne peut pas être dans le futur';
    END IF;

    -- La date de décès ne peut pas être dans le futur
    IF p_dateDeces IS NOT NULL AND p_dateDeces > CURRENT_DATE THEN
        RAISE EXCEPTION 'La date de décès ne peut pas être dans le futur';
    END IF;

    -- La date de décès ne peut pas être avant la date de naissance (contrainte table)
    IF p_dateDeces IS NOT NULL AND p_dateDeces < p_dateNaissance THEN
        RAISE EXCEPTION 'La date de décès ne peut pas être inférieure à la date de naissance';
    END IF;

    -- Un animal stérilisé doit avoir une date de stérilisation (contrainte table)
    IF p_sterilise IS TRUE AND p_dateSterilisation IS NULL THEN
        RAISE EXCEPTION 'Un animal stérilisé doit obligatoirement avoir une date de stérilisation';
    END IF;

    -- Un animal non stérilisé ne peut pas avoir une date de stérilisation (contrainte table)
    IF p_sterilise IS FALSE AND p_dateSterilisation IS NOT NULL THEN
        RAISE EXCEPTION 'Un animal non stérilisé ne peut pas avoir une date de stérilisation';
    END IF;

    -- La date de stérilisation ne peut pas être avant la date de naissance (contrainte table)
    IF p_dateSterilisation IS NOT NULL AND p_dateSterilisation < p_dateNaissance THEN
        RAISE EXCEPTION 'La date de stérilisation ne peut pas être inférieure à la date de naissance';
    END IF;

    --------------------------------------------------------------------
    -- 4) Génération automatique de l'identifiant
    --------------------------------------------------------------------
    -- Préfixe basé sur la date de naissance : YYMMDD
    prefix := to_char(p_dateNaissance, 'YYMMDD');

    -- Chercher le dernier identifiant existant pour ce préfixe
    SELECT identifiant
    INTO dernierId
    FROM animal a
    WHERE a.identifiant LIKE prefix || '%' -- rappel : tous les identifiants qui commencent par le prefix suivi de n'importe quoi après
    ORDER BY a.identifiant DESC
    LIMIT 1;

    IF NOT FOUND THEN
        seq := 1; -- premier animal né ce jour-là
    ELSE
        seq := (substring(dernierId from 7 for 5))::int + 1;
        -- la variable seq reçoit les cinq derniers caractères à partir du 7 (7ème compris) jusqu'au 11 ème convertir ces caractères en entier :: int et y ajoute 1
        -- le substring devant la parenthèse permet d'extraire la chaine voulue et :: int la converti en entier
    END IF;

    -- Construire l'identifiant final
    p_identifiant := prefix || LPAD(seq::text, 5, '0');
    -- Rappel : LPAD remplissage à gauche donc une fois seq converti en text via :: text, il ajoute des zéros jusqu'a trouvé des chiffres. Les zéro sont ajoutés
    -- sur cinq caractères
    -- finalement une concaténation des deux chaines est réalisées via l'opérateur || et la valeur affectée à la variable p_identifiant.

    --------------------------------------------------------------------
    -- 5) INSERT FINAL AVEC RETURNING
    --------------------------------------------------------------------
    INSERT INTO animal(
        identifiant,
        nom,
        type,
        sexe,
        sterilise,
        date_naissance,
        date_deces,
        date_sterilisation,
        particularites,
        description
    )
    VALUES (
        p_identifiant,
        p_nom,
        p_type,
        p_sexe,
        p_sterilise,
        p_dateNaissance,
        p_dateDeces,
        p_dateSterilisation,
        p_particularites,
        p_description
    )
    RETURNING identifiant INTO p_identifiant;  -- renvoie l'identifiant réel inséré

END;
$$;

alter procedure public.animal_insertion(out varchar, varchar, varchar, char, boolean, date, date, date, text, text) owner to postgres;

create or replace procedure public.animal_modification(IN p_identifiant character varying, IN p_nom character varying, IN p_type character varying, IN p_sexe character, IN p_sterilise boolean, IN p_datenaissance date, IN p_datedeces date, IN p_datesterilisation date, IN p_particularites text, IN p_description text)
    language plpgsql
as
$$
DECLARE
    existe animal%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier si l'animal existe via son identifiant
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM animal
    WHERE identifiant = p_identifiant;

    -- On lève une exception plutôt que de retourner un message
    -- afin que le try/catch C# intercepte naturellement l'erreur
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Aucun animal trouvé avec l''identifiant %', p_identifiant;
    END IF;

    --------------------------------------------------------------------
    -- 2) Vérifications métier sur les longueurs
    --------------------------------------------------------------------
    IF p_nom IS NULL OR LENGTH(TRIM(p_nom)) = 0 THEN
        RAISE EXCEPTION 'Le nom ne peut pas être vide';
    END IF;

    IF LENGTH(p_nom) > 50 THEN
        RAISE EXCEPTION 'Le nom ne peut pas dépasser 50 caractères';
    END IF;

    IF p_nom !~ '^[A-Za-zÀ-ÖØ-öø-ÿ\- ]+$' THEN
        RAISE EXCEPTION 'Le nom ne peut contenir que des lettres, espaces et tirets';
    END IF;

    IF p_type IS NULL OR LENGTH(TRIM(p_type)) = 0 THEN
        RAISE EXCEPTION 'Le type ne peut pas être vide';
    END IF;

    IF LENGTH(p_type) > 10 THEN
        RAISE EXCEPTION 'Le type ne peut pas dépasser 10 caractères';
    END IF;

    IF p_type NOT IN ('chien', 'chat') THEN
        RAISE EXCEPTION 'Seuls les types "chien" ou "chat" sont acceptés';
    END IF;

    IF p_sexe NOT IN ('M', 'F') THEN
        RAISE EXCEPTION 'Le sexe doit être M ou F';
    END IF;

    IF p_particularites IS NOT NULL AND LENGTH(p_particularites) > 500 THEN
        RAISE EXCEPTION 'Les particularités ne peuvent pas dépasser 500 caractères';
    END IF;

    IF p_description IS NOT NULL AND LENGTH(p_description) > 2000 THEN
        RAISE EXCEPTION 'La description ne peut pas dépasser 2000 caractères';
    END IF;

    --------------------------------------------------------------------
    -- 3) Vérifications métier sur les dates
    --------------------------------------------------------------------
    -- La date de naissance ne peut pas être dans le futur (contrainte table)
    IF p_dateNaissance > CURRENT_DATE THEN
        RAISE EXCEPTION 'La date de naissance ne peut pas être dans le futur';
    END IF;

    -- La date de décès ne peut pas être dans le futur
    IF p_dateDeces IS NOT NULL AND p_dateDeces > CURRENT_DATE THEN
        RAISE EXCEPTION 'La date de décès ne peut pas être dans le futur';
    END IF;

    -- La date de décès ne peut pas être avant la date de naissance (contrainte table)
    IF p_dateDeces IS NOT NULL AND p_dateDeces < p_dateNaissance THEN
        RAISE EXCEPTION 'La date de décès ne peut pas être inférieure à la date de naissance';
    END IF;

    -- Un animal stérilisé doit avoir une date de stérilisation (contrainte table)
    IF p_sterilise IS TRUE AND p_dateSterilisation IS NULL THEN
        RAISE EXCEPTION 'Un animal stérilisé doit obligatoirement avoir une date de stérilisation';
    END IF;

    -- Un animal non stérilisé ne peut pas avoir une date de stérilisation (contrainte table)
    IF p_sterilise IS FALSE AND p_dateSterilisation IS NOT NULL THEN
        RAISE EXCEPTION 'Un animal non stérilisé ne peut pas avoir une date de stérilisation';
    END IF;

    -- La date de stérilisation ne peut pas être avant la date de naissance (contrainte table)
    IF p_dateSterilisation IS NOT NULL AND p_dateSterilisation < p_dateNaissance THEN
        RAISE EXCEPTION 'La date de stérilisation ne peut pas être inférieure à la date de naissance';
    END IF;

    --------------------------------------------------------------------
    -- 4) L'animal existe et les vérifications sont passées → modification
    --------------------------------------------------------------------
    UPDATE animal
    SET nom                = p_nom,
        type               = p_type,
        sexe               = p_sexe,
        sterilise          = p_sterilise,
        date_naissance     = p_dateNaissance,
        date_deces         = p_dateDeces,
        date_sterilisation = p_dateSterilisation,
        particularites     = p_particularites,
        description        = p_description
    WHERE identifiant = p_identifiant;

END;
$$;

alter procedure public.animal_modification(varchar, varchar, varchar, char, boolean, date, date, date, text, text) owner to postgres;

create or replace procedure public.animal_suppression(IN p_identifiant character varying)
    language plpgsql
as
$$
DECLARE
    existe animal%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier si l'animal existe via son identifiant
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM animal
    WHERE identifiant = p_identifiant;

    -- On lève une exception plutôt que de retourner un message
    -- afin que le try/catch C# intercepte naturellement l'erreur
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Aucun animal trouvé avec l''identifiant %', p_identifiant;
    END IF;

    --------------------------------------------------------------------
    -- 2) L'animal existe → suppression
    -- Rappel : ON DELETE CASCADE sur les tables liées (ani_entree, ani_sortie,
    -- adoption, famille_accueil, vaccination, animal_couleur) supprimera
    -- automatiquement tous les enregistrements liés à cet animal.
    --------------------------------------------------------------------
    DELETE
    FROM animal
    WHERE identifiant = p_identifiant;

END;
$$;

alter procedure public.animal_suppression(varchar) owner to postgres;

create or replace function public.animal_afficher_liste_complete()
    returns TABLE(identifiant character varying, nom character varying, type character varying, sexe character, sterilise boolean, date_naissance date, date_deces date, date_sterilisation date, particularites text, description text)
    language plpgsql
as
$$
BEGIN

    RETURN QUERY
        SELECT a.identifiant,
               a.nom,
               a.type,
               a.sexe,
               a.sterilise,
               a.date_naissance,
               a.date_deces,
               a.date_sterilisation,
               a.particularites,
               a.description
        FROM animal a;

END;
$$;

alter function public.animal_afficher_liste_complete() owner to postgres;

create or replace function public.animal_afficher_un(p_identifiant character varying)
    returns TABLE(identifiant character varying, nom character varying, type character varying, sexe character, sterilise boolean, date_naissance date, date_deces date, date_sterilisation date, particularites text, description text, couleur character varying)
    language plpgsql
as
$$
DECLARE
    existe animal%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier que l'animal existe
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM animal
    WHERE identifiant = p_identifiant;

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'L''identifiant % n''est pas présent dans la liste du refuge',
            p_identifiant;
    END IF;

    --------------------------------------------------------------------
    -- 2) Retourner l'animal + ses couleurs
    --------------------------------------------------------------------
    RETURN QUERY
        SELECT a.identifiant,
               a.nom,
               a.type,
               a.sexe,
               a.sterilise,
               a.date_naissance,
               a.date_deces,
               a.date_sterilisation,
               a.particularites,
               a.description,
               c.nom_couleur AS couleur
        FROM animal a
                 LEFT JOIN animal_couleur ac                                 -- La left join est pertinente ici, car si l'animal n'a aucune couleur la jointure ne retournera rien
                           ON ac.ani_identifiant = a.identifiant    -- alors que l'animal existe. Sans cela le DAO "pensera" que l'animal n'existe pas alors qu'il figure
                 LEFT JOIN couleur c                                         -- dans la liste.
                           ON c.col_identifiant = ac.col_identifiant
        WHERE a.identifiant = p_identifiant;

END;
$$;

alter function public.animal_afficher_un(varchar) owner to postgres;

