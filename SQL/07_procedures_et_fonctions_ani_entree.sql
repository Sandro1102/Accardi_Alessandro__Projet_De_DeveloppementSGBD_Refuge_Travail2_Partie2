create or replace procedure public.ani_entree_insertion(IN p_raison character varying, IN p_date_entree date, IN p_identifiant_animal character varying, IN p_identifiant_contact integer)
    language plpgsql
as
$$
DECLARE
    existe_animal   animal%ROWTYPE;
    existe_contact  contact%ROWTYPE;
    derniere_entree DATE;
    derniere_sortie DATE;
BEGIN
    -- Vérification animal
    SELECT *
    INTO existe_animal
    FROM animal
    WHERE identifiant = p_identifiant_animal;

    IF NOT FOUND THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion entrée impossible : animal %s introuvable.',
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
                'Insertion entrée impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut plus avoir de nouvelles entrées
    IF existe_animal.date_deces IS NOT NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion entrée impossible : animal %s est décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Contrainte : un animal ne peut être entré qu'une fois depuis sa dernière sortie
    -- c'est-à-dire qu'il ne peut pas avoir une entrée plus récente que sa dernière sortie
    SELECT MAX(date_entree)
    INTO derniere_entree
    FROM ani_entree
    WHERE ani_identifiant = p_identifiant_animal;

    SELECT MAX(date_sortie)
    INTO derniere_sortie
    FROM ani_sortie
    WHERE ani_identifiant = p_identifiant_animal;

    IF derniere_entree IS NOT NULL AND (derniere_sortie IS NULL OR derniere_entree > derniere_sortie) THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion entrée impossible : animal %s est déjà présent au refuge.',
                p_identifiant_animal
                                        );
    END IF;

    -- Insertion
    INSERT INTO ani_entree (raison,
                            date_entree,
                            ani_identifiant,
                            entree_contact)
    VALUES (p_raison,
            p_date_entree,
            p_identifiant_animal,
            p_identifiant_contact);
END;
$$;

alter procedure public.ani_entree_insertion(varchar, date, varchar, integer) owner to postgres;

