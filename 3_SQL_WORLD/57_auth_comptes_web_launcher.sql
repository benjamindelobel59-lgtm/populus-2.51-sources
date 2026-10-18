-- MYSTIC 18/10/2026 : SQL 57 — base `auth`
-- Tables des comptes web (site + launcher), sessions du launcher et partage de comptes par code.
-- Sans risque : « IF NOT EXISTS ». L'AuthServer et le site les créent aussi tout seuls au besoin.
USE `auth`;

CREATE TABLE IF NOT EXISTS nekfeu_comptes_web (
    Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, Email VARCHAR(190) NOT NULL, Pseudo VARCHAR(40) NOT NULL,
    MotDePasse VARCHAR(255) NOT NULL, CreeLe DATETIME NOT NULL, DerniereConnexion DATETIME NULL,
    Bloque TINYINT NOT NULL DEFAULT 0, UNIQUE KEY (Email), UNIQUE KEY (Pseudo)) DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS nekfeu_comptes_web_jeu (
    AccountId INT NOT NULL PRIMARY KEY, CompteWebId INT NOT NULL, LieLe DATETIME NOT NULL, KEY (CompteWebId)) DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS nekfeu_web_sessions (
    Empreinte CHAR(64) NOT NULL PRIMARY KEY, CompteWebId INT NOT NULL, CreeLe DATETIME NOT NULL, ExpireLe DATETIME NOT NULL,
    DerniereIp VARCHAR(64) NOT NULL DEFAULT '', KEY (CompteWebId)) DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS nekfeu_partages (
    AccountId INT NOT NULL, CompteWebId INT NOT NULL, ProprietaireWebId INT NOT NULL, CreeLe DATETIME NOT NULL,
    PRIMARY KEY (AccountId, CompteWebId), KEY (CompteWebId), KEY (ProprietaireWebId)) DEFAULT CHARSET=utf8;

CREATE TABLE IF NOT EXISTS nekfeu_partages_codes (
    Empreinte CHAR(64) NOT NULL PRIMARY KEY, ProprietaireWebId INT NOT NULL, Comptes VARCHAR(255) NOT NULL,
    ExpireLe DATETIME NOT NULL, KEY (ProprietaireWebId)) DEFAULT CHARSET=utf8;
