create or replace procedure public.ani_sortie_insertion(IN p_raison character varying, IN p_date_sortie date, IN p_identifiant_animal character varying, IN p_identifiant_contact integer)
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
                'Insertion sortie impossible : animal %s introuvable.',
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
                'Insertion sortie impossible : contact %s introuvable.',
                p_identifiant_contact
                                        );
    END IF;

    -- Contrainte : un animal décédé ne peut plus avoir de nouvelles sorties
    IF existe_animal.date_deces IS NOT NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion sortie impossible : animal %s est décédé le %s.',
                p_identifiant_animal,
                existe_animal.date_deces
                                        );
    END IF;

    -- Récupération de la dernière entrée et de la dernière sortie
    SELECT MAX(date_entree)
    INTO derniere_entree
    FROM ani_entree
    WHERE ani_identifiant = p_identifiant_animal;

    SELECT MAX(date_sortie)
    INTO derniere_sortie
    FROM ani_sortie
    WHERE ani_identifiant = p_identifiant_animal;

    -- Contrainte : chaque animal doit avoir au moins une entrée avant de pouvoir sortir
    IF derniere_entree IS NULL THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion sortie impossible : animal %s n''a aucune entrée enregistrée.',
                p_identifiant_animal
                                        );
    END IF;

    -- Contrainte : il ne peut y avoir qu''une seule sortie depuis la dernière entrée
    IF derniere_sortie IS NOT NULL AND derniere_sortie >= derniere_entree THEN
        RAISE EXCEPTION USING MESSAGE = FORMAT(
                'Insertion sortie impossible : animal %s est déjà sorti du refuge.',
                p_identifiant_animal
                                        );
    END IF;

    -- Insertion
    INSERT INTO ani_sortie (raison,
                            date_sortie,
                            ani_identifiant,
                            sortie_contact)
    VALUES (p_raison,
            p_date_sortie,
            p_identifiant_animal,
            p_identifiant_contact);
END;
$$;

alter procedure public.ani_sortie_insertion(varchar, date, varchar, integer) owner to postgres;

