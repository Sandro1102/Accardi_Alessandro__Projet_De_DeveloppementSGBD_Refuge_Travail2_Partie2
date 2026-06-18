create or replace procedure public.personne_role_insertion(IN p_identifiant_contact integer, IN p_identifiant_role integer)
    language plpgsql
as
$$
DECLARE
    existe_contact contact%ROWTYPE;
    existe_role    role%ROWTYPE;
BEGIN
    -- Vérification contact
    SELECT *
    INTO existe_contact
    FROM contact
    WHERE contact_identifiant = p_identifiant_contact;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Vérification rôle
    SELECT *
    INTO existe_role
    FROM role
    WHERE rol_identifiant = p_identifiant_role;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion impossible : rôle %s introuvable.',
                p_identifiant_role
                                        );
    END IF;

    -- Insertion
    INSERT INTO personne_role (pers_identifiant,
                               rol_identifiant)
    VALUES (p_identifiant_contact,
            p_identifiant_role);
END;
$$;

alter procedure public.personne_role_insertion(integer, integer) owner to postgres;

create or replace procedure public.personne_role_suppression(IN p_identifiant_contact integer, IN p_identifiant_role integer)
    language plpgsql
as
$$
DECLARE
    existe personne_role%ROWTYPE;
BEGIN
    -- Vérification existence
    SELECT *
    INTO existe
    FROM personne_role
    WHERE pers_identifiant = p_identifiant_contact
      AND rol_identifiant = p_identifiant_role;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Suppression impossible : aucune association contact=%s, rôle=%s.',
                p_identifiant_contact, p_identifiant_role
                                        );
    END IF;

    -- Suppression
    DELETE
    FROM personne_role
    WHERE pers_identifiant = p_identifiant_contact
      AND rol_identifiant = p_identifiant_role;
END;
$$;

alter procedure public.personne_role_suppression(integer, integer) owner to postgres;

