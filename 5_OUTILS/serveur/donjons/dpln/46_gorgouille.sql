-- MYSTIC 18/10 : Antre du Dragon Cochon : salle 11 de la Gorgouille (carte 72359936, salle spéciale : 1 seule case bleue au centre
-- pour elle, 8 cases rouges autour pour les joueurs). Parcours : salle 72355840 → Gorgouille → Dragon Cochon (72361984).
-- Annulation : 46_gorgouille-annuler.sql
INSERT INTO monsters_spawns_dungeons (MapId,CellId,TeleportEvent,TeleportMapId,TeleportCell,TeleportDirection,Descripcion)
VALUES (72359936,NULL,1,72361984,430,7,'MYSTIC 46 Dragon Cochon salle Gorgouille - boss');
SET @g = LAST_INSERT_ID();
INSERT INTO monsters_spawns_dungeons_groups (DungeonSpawnId,MonsterGradeId,MinPartyMembers,Informations) VALUES
  (@g,7182,1,'MYSTIC 46 gorgouille'),(@g,7183,5,'MYSTIC 46 gorgouille'),(@g,7184,6,'MYSTIC 46 gorgouille'),
  (@g,7185,7,'MYSTIC 46 gorgouille'),(@g,7186,8,'MYSTIC 46 gorgouille');
UPDATE monsters_spawns_dungeons SET TeleportMapId=72359936, TeleportCell=371 WHERE Id=159;
