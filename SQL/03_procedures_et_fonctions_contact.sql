CREATE PROCEDURE contact_insertion(
    OUT p_contact_identifiant   INT,
    IN  p_nom                   CHARACTER VARYING,
    IN  p_prenom                CHARACTER VARYING,
    IN  p_registre_national     CHARACTER VARYING,
    IN  p_rue                   CHARACTER VARYING,
    IN  p_cp                    CHARACTER VARYING,
    IN  p_localite              CHARACTER VARYING,
    IN  p_gsm                   CHARACTER VARYING,
    IN  p_telephone             CHARACTER VARYING,
    IN  p_email                 CHARACTER VARYING
)
LANGUAGE plpgsql
AS $$
DECLARE
    existe contact%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier si le contact existe déjà via son registre national
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM contact c
    WHERE c.registre_national = p_registre_national;

    IF FOUND THEN
        RAISE EXCEPTION USING
            MESSAGE = 'Un contact avec ce registre national existe déjà',
            DETAIL  = FORMAT('Il possède l''identifiant suivant : %s', existe.contact_identifiant);
    END IF;

    --------------------------------------------------------------------
    -- 2) Vérifications métier sur les champs obligatoires
    --------------------------------------------------------------------
    IF p_nom IS NULL OR LENGTH(TRIM(p_nom)) < 2 THEN
        RAISE EXCEPTION 'Le nom doit avoir une longueur de minimum deux caractères';
    END IF;

    IF LENGTH(p_nom) > 50 THEN
        RAISE EXCEPTION 'Le nom ne peut pas dépasser 50 caractères';
    END IF;

    IF p_nom ~ '\d' THEN
        RAISE EXCEPTION 'Le nom ne peut pas contenir de chiffres';
    END IF;

    IF p_prenom IS NULL OR LENGTH(TRIM(p_prenom)) < 2 THEN
        RAISE EXCEPTION 'Le prénom doit avoir une longueur de minimum deux caractères';
    END IF;

    IF LENGTH(p_prenom) > 50 THEN
        RAISE EXCEPTION 'Le prénom ne peut pas dépasser 50 caractères';
    END IF;

    IF p_prenom ~ '\d' THEN
        RAISE EXCEPTION 'Le prénom ne peut pas contenir de chiffres';
    END IF;

    IF p_registre_national IS NULL OR LENGTH(TRIM(p_registre_national)) = 0 THEN
        RAISE EXCEPTION 'Le registre national ne peut pas être vide';
    END IF;

    -- Format attendu : xx.xx.xx-xxx.xx (contrainte table)
    IF p_registre_national !~ '^\d{2}\.\d{2}\.\d{2}-\d{3}\.\d{2}$' THEN
        RAISE EXCEPTION 'Format du registre national invalide. Format attendu : yy.MM.dd-xxx.xx (ex : 89.03.25-345.23)';
    END IF;

    --------------------------------------------------------------------
    -- 3) Vérifications métier sur les champs optionnels
    --------------------------------------------------------------------
    IF p_rue IS NOT NULL AND LENGTH(TRIM(p_rue)) < 2 THEN
        RAISE EXCEPTION 'Le nom de rue invalide';
    END IF;

    IF p_rue IS NOT NULL AND LENGTH(p_rue) > 100 THEN
        RAISE EXCEPTION 'Le nom de rue ne peut pas dépasser 100 caractères';
    END IF;

    IF p_cp IS NOT NULL AND p_cp !~ '^\d+$' THEN
        RAISE EXCEPTION 'Le code postal ne peut contenir que des chiffres';
    END IF;

    IF p_cp IS NOT NULL AND LENGTH(p_cp) > 10 THEN
        RAISE EXCEPTION 'Le code postal ne peut pas dépasser 10 caractères';
    END IF;

    IF p_localite IS NOT NULL AND LENGTH(TRIM(p_localite)) < 2 THEN
        RAISE EXCEPTION 'Le nom de localité est invalide';
    END IF;

    IF p_localite IS NOT NULL AND LENGTH(p_localite) > 50 THEN
        RAISE EXCEPTION 'Le nom de localité ne peut pas dépasser 50 caractères';
    END IF;

    -- Format GSM belge : 10 chiffres commençant par 04
    IF p_gsm IS NOT NULL AND p_gsm !~ '^04\d{8}$' THEN
        RAISE EXCEPTION 'Le GSM doit être au format belge : 0485359516';
    END IF;

    -- Format téléphone fixe belge : 9 chiffres commençant par 0
    IF p_telephone IS NOT NULL AND p_telephone !~ '^0\d{8}$' THEN
        RAISE EXCEPTION 'Le téléphone fixe doit comporter 9 chiffres et commencer par 0 (ex: 041234567)';
    END IF;

    -- Format email (contrainte table)
    IF p_email IS NOT NULL AND p_email !~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' THEN
        RAISE EXCEPTION 'L''adresse email ne respecte pas le format requis (ex: contact@domaine.com)';
    END IF;

    -- Au moins un moyen de contact obligatoire (contrainte table)
    IF p_gsm IS NULL AND p_telephone IS NULL AND p_email IS NULL THEN
        RAISE EXCEPTION 'Au moins un moyen de contact doit être renseigné : GSM, téléphone fixe ou email';
    END IF;

    --------------------------------------------------------------------
    -- 4) INSERT FINAL AVEC RETURNING
    --------------------------------------------------------------------
    INSERT INTO contact(
        nom,
        prenom,
        registre_national,
        rue,
        cp,
        localite,
        gsm,
        telephone,
        email
    )
    VALUES (
        p_nom,
        p_prenom,
        p_registre_national,
        p_rue,
        p_cp,
        p_localite,
        p_gsm,
        p_telephone,
        p_email
    )
    RETURNING contact_identifiant INTO p_contact_identifiant;  -- renvoie l'id généré par le SERIAL

END;
$$;


CREATE PROCEDURE contact_modification(
    IN p_registre_national  CHARACTER VARYING,
    IN p_nom                CHARACTER VARYING,
    IN p_prenom             CHARACTER VARYING,
    IN p_rue                CHARACTER VARYING,
    IN p_cp                 CHARACTER VARYING,
    IN p_localite           CHARACTER VARYING,
    IN p_gsm                CHARACTER VARYING,
    IN p_telephone          CHARACTER VARYING,
    IN p_email              CHARACTER VARYING
)
LANGUAGE plpgsql
AS $$
DECLARE
    existe contact%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier si le contact existe via son registre national
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM contact c
    WHERE c.registre_national = p_registre_national;

    -- On lève une exception plutôt que de retourner un message
    -- afin que le try/catch C# intercepte naturellement l'erreur
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Aucun contact trouvé avec le registre national %', p_registre_national;
    END IF;

    --------------------------------------------------------------------
    -- 2) Vérifications métier sur les champs obligatoires
    --------------------------------------------------------------------
    IF p_nom IS NULL OR LENGTH(TRIM(p_nom)) < 2 THEN
        RAISE EXCEPTION 'Le nom doit avoir une longueur de minimum deux caractères';
    END IF;

    IF LENGTH(p_nom) > 50 THEN
        RAISE EXCEPTION 'Le nom ne peut pas dépasser 50 caractères';
    END IF;

    IF p_nom ~ '\d' THEN
        RAISE EXCEPTION 'Le nom ne peut pas contenir de chiffres';
    END IF;

    IF p_prenom IS NULL OR LENGTH(TRIM(p_prenom)) < 2 THEN
        RAISE EXCEPTION 'Le prénom doit avoir une longueur de minimum deux caractères';
    END IF;

    IF LENGTH(p_prenom) > 50 THEN
        RAISE EXCEPTION 'Le prénom ne peut pas dépasser 50 caractères';
    END IF;

    IF p_prenom ~ '\d' THEN
        RAISE EXCEPTION 'Le prénom ne peut pas contenir de chiffres';
    END IF;

    --------------------------------------------------------------------
    -- 3) Vérifications métier sur les champs optionnels
    --------------------------------------------------------------------
    IF p_rue IS NOT NULL AND LENGTH(TRIM(p_rue)) < 2 THEN
        RAISE EXCEPTION 'Le nom de rue est invalide';
    END IF;

    IF p_rue IS NOT NULL AND LENGTH(p_rue) > 100 THEN
        RAISE EXCEPTION 'Le nom de rue ne peut pas dépasser 100 caractères';
    END IF;

    IF p_cp IS NOT NULL AND p_cp !~ '^\d+$' THEN
        RAISE EXCEPTION 'Le code postal ne peut contenir que des chiffres';
    END IF;

    IF p_cp IS NOT NULL AND LENGTH(p_cp) > 10 THEN
        RAISE EXCEPTION 'Le code postal ne peut pas dépasser 10 caractères';
    END IF;

    IF p_localite IS NOT NULL AND LENGTH(TRIM(p_localite)) < 2 THEN
        RAISE EXCEPTION 'Le nom de localité est invalide';
    END IF;

    IF p_localite IS NOT NULL AND LENGTH(p_localite) > 50 THEN
        RAISE EXCEPTION 'Le nom de localité ne peut pas dépasser 50 caractères';
    END IF;

    -- Format GSM belge : 10 chiffres commençant par 04
    IF p_gsm IS NOT NULL AND p_gsm !~ '^04\d{8}$' THEN
        RAISE EXCEPTION 'Le GSM doit être au format belge : 0485359516';
    END IF;

    -- Format téléphone fixe belge : 9 chiffres commençant par 0
    IF p_telephone IS NOT NULL AND p_telephone !~ '^0\d{8}$' THEN
        RAISE EXCEPTION 'Le téléphone fixe doit comporter 9 chiffres et commencer par 0 (ex: 041234567)';
    END IF;

    -- Format email (contrainte table)
    IF p_email IS NOT NULL AND p_email !~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' THEN
        RAISE EXCEPTION 'L''adresse email ne respecte pas le format requis (ex: contact@domaine.com)';
    END IF;

    -- Au moins un moyen de contact obligatoire (contrainte table)
    IF p_gsm IS NULL AND p_telephone IS NULL AND p_email IS NULL THEN
        RAISE EXCEPTION 'Au moins un moyen de contact doit être renseigné : GSM, téléphone fixe ou email';
    END IF;

    --------------------------------------------------------------------
    -- 4) Le contact existe et les vérifications sont passées → modification
    --------------------------------------------------------------------
    UPDATE contact
    SET nom               = p_nom,
        prenom            = p_prenom,
        rue               = p_rue,
        cp                = p_cp,
        localite          = p_localite,
        gsm               = p_gsm,
        telephone         = p_telephone,
        email             = p_email
    WHERE registre_national = p_registre_national;

END;
$$;


CREATE PROCEDURE contact_suppression(
    IN p_registre_national CHARACTER VARYING
)
LANGUAGE plpgsql
AS $$
DECLARE
    existe contact%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier si le contact existe via son registre national
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM contact c
    WHERE c.registre_national = p_registre_national;

    -- On lève une exception plutôt que de retourner un message
    -- afin que le try/catch C# intercepte naturellement l'erreur
    IF NOT FOUND THEN
        RAISE EXCEPTION 'Aucun contact trouvé avec le registre national %', p_registre_national;
    END IF;

    --------------------------------------------------------------------
    -- 2) Le contact existe → suppression
    -- Rappel : ON DELETE CASCADE sur personne_role supprimera automatiquement
    -- tous les rôles liés à ce contact
    --------------------------------------------------------------------
    DELETE
    FROM contact
    WHERE registre_national = p_registre_national;

END;
$$;


CREATE FUNCTION contact_afficher_liste_complete()
    RETURNS TABLE
            (
                contact_identifiant INT,
                nom                 VARCHAR,
                prenom              VARCHAR,
                registre_national   VARCHAR,
                rue                 VARCHAR,
                cp                  VARCHAR,
                localite            VARCHAR,
                gsm                 VARCHAR,
                telephone           VARCHAR,
                email               VARCHAR
            )
    LANGUAGE plpgsql
AS
$$
BEGIN

    RETURN QUERY
        SELECT
            c.contact_identifiant,
            c.nom,
            c.prenom,
            c.registre_national,
            c.rue,
            c.cp,
            c.localite,
            c.gsm,
            c.telephone,
            c.email
        FROM contact c;

END;
$$;


CREATE FUNCTION contact_afficher_un(p_registre_national VARCHAR)
    RETURNS TABLE
            (
                contact_identifiant INT,
                nom                 VARCHAR,
                prenom              VARCHAR,
                registre_national   VARCHAR,
                rue                 VARCHAR,
                cp                  VARCHAR,
                localite            VARCHAR,
                gsm                 VARCHAR,
                telephone           VARCHAR,
                email               VARCHAR,
                role                VARCHAR
            )
    LANGUAGE plpgsql
AS
$$
DECLARE
    existe contact%ROWTYPE;
BEGIN
    --------------------------------------------------------------------
    -- 1) Vérifier que le contact existe
    --------------------------------------------------------------------
    SELECT *
    INTO existe
    FROM contact c
    WHERE c.registre_national = p_registre_national;

    IF NOT FOUND THEN
        RAISE EXCEPTION
            'Le registre national % n''est pas présent dans la liste des contacts',
            p_registre_national;
    END IF;

    --------------------------------------------------------------------
    -- 2) Retourner le contact + ses rôles
    --------------------------------------------------------------------
    RETURN QUERY
    SELECT
        c.contact_identifiant,
        c.nom,
        c.prenom,
        c.registre_national,
        c.rue,
        c.cp,
        c.localite,
        c.gsm,
        c.telephone,
        c.email,
        r.rol_nom AS role
    FROM contact c
    LEFT JOIN personne_role pr                              -- La left join est pertinente ici, car si le contact n'a aucun rôle la jointure ne retournera rien
           ON pr.pers_identifiant = c.contact_identifiant  -- alors que le contact existe. Sans cela le DAO "pensera" que le contact n'existe pas alors qu'il figure
    LEFT JOIN role r                                        -- dans la liste.
           ON r.rol_identifiant = pr.rol_identifiant
    WHERE c.registre_national = p_registre_national;

END;
$$;