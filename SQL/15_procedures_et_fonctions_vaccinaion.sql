create or replace procedure public.vaccination_insertion(IN p_date date, IN p_identifiant_animal character varying, IN p_identifiant_vaccin integer)
    language plpgsql
as
$$
DECLARE
    existe_animal animal%ROWTYPE;
    existe_vaccin vaccin%ROWTYPE;
    existe        vaccination%ROWTYPE;
BEGIN
    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion vaccination impossible : animal %s introuvable.',
                p_identifiant_animal
                                        );
    END IF;

    -- Vérification vaccin
    SELECT *
    INTO existe_vaccin
    FROM vaccin
    WHERE identifiant = p_identifiant_vaccin;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion vaccination impossible : vaccin %s introuvable.',
                p_identifiant_vaccin
                                        );
    END IF;

    -- Contrainte : la date de vaccination doit être supérieure à la date de naissance
    IF p_date < existe_animal.date_naissance THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion vaccination impossible : la date %s est antérieure à la naissance de l''animal %s.',
                p_date,
                p_identifiant_animal
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut plus être vacciné
    IF existe_animal.date_deces IS NOT NULL AND p_date > existe_animal.date_deces THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Impossible de vacciner : animal %s décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Contrainte : pas deux fois le même vaccin le même jour
    SELECT *
    INTO existe
    FROM vaccination
    WHERE vac_animal = p_identifiant_animal
      AND id_vaccin = p_identifiant_vaccin
      AND vaccination_date = p_date;

    IF FOUND THEN
        -- Correction : apostrophe échappée dans l''animal
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Impossible de vacciner : vaccin %s déjà administré à l''animal %s le %s.',
                p_identifiant_vaccin,
                p_identifiant_animal,
                p_date
                                        );
    END IF;

    -- Insertion
    INSERT INTO vaccination (vaccination_date,
                             vac_animal,
                             id_vaccin)
    VALUES (p_date,
            p_identifiant_animal,
            p_identifiant_vaccin);
END;
$$;

alter procedure public.vaccination_insertion(date, varchar, integer) owner to postgres;

create or replace procedure public.vaccination_suppression(IN p_date date, IN p_identifiant_animal character varying, IN p_identifiant_vaccin integer)
    language plpgsql
as
$$
DECLARE
    existe vaccination%ROWTYPE;
BEGIN
    -- Vérification existence
    SELECT *
    INTO existe
    FROM vaccination
    WHERE vac_animal = p_identifiant_animal
      AND id_vaccin = p_identifiant_vaccin
      AND vaccination_date = p_date;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Suppression impossible : aucune vaccination trouvée pour animal=%s, vaccin=%s, date=%s.',
                p_identifiant_animal, p_identifiant_vaccin, p_date
                                        );
    END IF;

    -- Suppression
    DELETE
    FROM vaccination
    WHERE vac_animal = p_identifiant_animal
      AND id_vaccin = p_identifiant_vaccin
      AND vaccination_date = p_date;
END;
$$;

alter procedure public.vaccination_suppression(date, varchar, integer) owner to postgres;

