"""Rewrite the Ankama/Mustam menuadmin.xml so that it only contains commands this Stump server understands.

usage: convert_menuadmin.py <in.xml> <out.xml>
"""
import re
import subprocess
import sys
import xml.etree.ElementTree as ET
from collections import Counter

MYSQL = r"C:\xampp\mysql\bin\mysql.exe"


def ids(sql):
    r = subprocess.run([MYSQL, "-uroot", "-N", "-B", "world", "-e", sql], capture_output=True, text=True)
    return {int(x) for x in r.stdout.split()}


MAPS = ids("SELECT Id FROM world_maps")
ITEMS = ids("SELECT Id FROM items_templates UNION SELECT Id FROM items_templates_weapons")


def dungeon_by_key():
    """key item -> map in front of the dungeon (keeper map), or its first room."""
    out = {}
    r = subprocess.run([MYSQL, "-uroot", "-N", "-B", "world", "-e",
                        "SELECT e.KeyItemId, IF(e.EntranceMapId IN (SELECT Id FROM world_maps), e.EntranceMapId, e.FirstRoomMapId) "
                        "FROM dungeons_entries e WHERE e.KeyItemId > 0"], capture_output=True, text=True)
    for line in r.stdout.splitlines():
        if line.strip():
            k, m = line.split()
            out[int(k)] = int(m)
    dungeons = {}
    r = subprocess.run([MYSQL, "-uroot", "-N", "-B", "world", "-e",
                        "SELECT Id, IFNULL(EntranceMapId,0), IFNULL(FirstRoom,0) FROM dungeons"], capture_output=True, text=True)
    for line in r.stdout.splitlines():
        if line.strip():
            d, e, f = (int(x) for x in line.split())
            dungeons[d] = e if e in MAPS else (f if f in MAPS else 0)
    for line in open(KEYS_FILE):
        d, k = (int(x) for x in line.split())
        if k > 0 and dungeons.get(d) and k not in out:
            out[k] = dungeons[d]
    return out


KEYS_FILE = sys.argv[3] if len(sys.argv) > 3 else "dj_keys.tsv"
DUNGEON_BY_KEY = dungeon_by_key()

# exact command (after trim) -> new command, or (type, command) to change the item type
EXACT = {
    "align %p 0": "alignment side 0 %p",
    "join %p": "goname %p",
    "mapinfo": "info map",
    "cellinfo": "porte info",
    "unmute %p": "unmute %p",
    "disconnect %p": "kick %p",
    "cancelfight mine": "fight end",
    "admininvisible true": "invisible %n",
    "god": "god on %n",
    "pausefight pause": "fight freeze",
    "pausefight resume": "fight freeze",
    "passturn": "fight pass",
    "showinteractives": "interactives show",
    "showbank %p": "bank open %p",
    "showinv %p": "item showinv %p",
    "title %p all": "title add %p -all",
    "ornament %p all": "ornament add %p -all",
    "addsummonedmonster 4x494": "monster spawn 494",
    "addsummonedmonster 1x#monster 1": "monster spawn #monster",
    "look %p #look": "look #look %p",
    "item %p #item": "item add #item %p 1",
    "reloadnpcs": "reload npcs",
    "reloadinteractives": "reload interactives",
    "reloadmapmonsters": "reload monsters",
    "forcedump": "save world",
    "givelevel %p 200": "levelup 199 %p",
    "givegold %p 100000000": "kamas 100000000 %p",
}

# prepareCommand : the text is put in the console, the admin types the rest
PREPARE = {
    "move %p": "go -target=%p ",
    "look %p": "look -target=%p ",
    "givelevel %p": "levelup -target=%p ",
    "givespell %p": "spell learn -target=%p ",
    "align %p": "alignment side -target=%p ",
    "item %p": "item add -target=%p ",
    "givegold %p": "kamas -target=%p ",
    "addsummonedmonster": "monster spawn ",
    "mute %p": "mute %p ",
    "kick %p": "kick %p",
    "achievement %p": "achievements add ",
    "advertise": "announce ",
    "title %p": "title add %p ",
    "ornament %p": "ornament add %p ",
    "addnpcmap": "npcs spawn ",
    "removenpcmap": "npcs unspawn ",
}

LABELS = {
    "Invisible ON": "Invisible (on/off)",
    "God Mode (toggle)": "God Mode ON",
    "Pause combat": "Pause / reprise du combat",
    "Déconnecter joueur": "Déconnecter (kick)",
    "Recharger monstres carte": "Recharger monstres",
}

stats = Counter()


def target(tok):
    # "*" alone means every connected player on this server : the Ankama menu used it for "myself"
    return "%n" if tok == "*" else tok


def translate(cmd):
    cmd = (cmd or "").strip()
    if cmd in EXACT:
        return EXACT[cmd]

    m = re.fullmatch(r"align (\S+) ([12])(?: \d+ \d+)?", cmd)
    if m:
        return f"alignment side {m.group(2)} {m.group(1)}"

    m = re.fullmatch(r"move (\S+) (-?\d+)(?: (\d+))?", cmd)
    if m:
        if int(m.group(2)) not in MAPS:
            stats["map absente"] += 1
            return None
        cell = f" {m.group(3)}" if m.group(3) else ""
        return f"go {m.group(2)} {target(m.group(1))}{cell}"

    m = re.fullmatch(r"item (\S+) (\d+)(?: (\d+))?", cmd)
    if m:
        if int(m.group(2)) not in ITEMS:
            stats["objet absent"] += 1
            return None
        return f"item add {m.group(2)} {target(m.group(1))} {m.group(3) or 1}"

    stats["sans équivalent: " + (cmd.split() or ["?"])[0]] += 1
    return None


def convert(node):
    """Converts the children of node in place. Returns True when node still holds something to click."""
    useful = False
    for child in list(node):
        if child.tag != "item":
            node.remove(child)
            continue
        kind = (child.get("type") or "").lower()
        child.attrib.pop("rank", None)

        if kind in ("startup", "sendchat"):
            node.remove(child)
            continue

        if kind == "menu":
            keep = convert(child)
        elif kind == "batch":
            lost_move = False
            for sub in list(child):
                old = (sub.get("command") or "").strip()
                new = translate(old)
                if new is None:
                    lost_move |= old.startswith("move ")
                    child.remove(sub)
                else:
                    sub.set("command", new)
                    sub.attrib.pop("rank", None)
            if lost_move:
                # the 2.33 map does not exist here : go in front of the dungeon found from its key instead
                keys = [int(c.get("command").split()[2]) for c in child if c.get("command", "").startswith("item add ")]
                dest = next((DUNGEON_BY_KEY[k] for k in keys if k in DUNGEON_BY_KEY), None)
                if dest:
                    child.append(ET.Element("item", {"type": "sendCommand", "command": f"go {dest} %n"}))
                    stats["donjon remplacé par la map de ta base"] += 1
                else:
                    # never leave a lone key or a half batch
                    for sub in list(child):
                        child.remove(sub)
                    stats["lot retiré (map absente)"] += 1
            keep = len(child) > 0
        elif kind == "sendcommand":
            cmd = (child.get("command") or "").strip()
            new = translate(cmd)
            if new is None and cmd in PREPARE:
                child.set("type", "prepareCommand")
                new = PREPARE[cmd]
            if new is not None:
                child.set("command", new)
            keep = new is not None
        elif kind == "preparecommand":
            new = PREPARE.get((child.get("command") or "").strip())
            if new is not None:
                child.set("command", new)
            else:
                stats["prepare sans équivalent: " + ((child.get("command") or "?").split() or ["?"])[0]] += 1
            keep = new is not None
        else:  # static, separator, loadXML
            if kind == "loadxml":
                useful = True
            continue

        if not keep:
            node.remove(child)
            continue
        if child.get("label") in LABELS:
            child.set("label", LABELS[child.get("label")])
        useful = True

    # menu left with only its title / separators
    if not useful:
        return False
    return True


def add_after(parent, label, new_items):
    children = list(parent)
    for i, c in enumerate(children):
        if c.get("label") == label:
            for j, item in enumerate(new_items):
                parent.insert(i + 1 + j, item)
            return True
    return False


def item(kind, label, command=None):
    e = ET.Element("item", {"type": kind, "label": label})
    if command is not None:
        e.set("command", command)
    return e


def main(src, dst):
    text = open(src, encoding="utf-8").read()
    text = re.sub(r"^\s*<\?xml[^>]*\?>", "", text.lstrip("\ufeff"))
    text = re.sub(r"<!--.*?-->", "", text, flags=re.S)
    root = ET.fromstring(text)

    convert(root)

    for menu in root.iter("item"):
        if menu.get("label") == "Mode Admin":
            add_after(menu, "God Mode ON", [item("sendCommand", "God Mode OFF", "god off %n")])

    # this server's own tools
    populus = item("menu", "Outils Populus")
    for kind, label, command in [
        ("static", "Outils Populus", None),
        ("sendCommand", "Triggers de la map", "porte info"),
        ("sendCommand", "Porte : début (case extérieure)", "porte debut"),
        ("sendCommand", "Porte : fin (arrivée intérieure)", "porte fin"),
        ("sendCommand", "Porte : sortie (case intérieure)", "porte sortie"),
        ("sendCommand", "Porte : sortie de maison", "porte sortiemaison"),
        ("sendCommand", "Supprimer le trigger de ma case", "porte supprimer"),
        ("separator", "", None),
        ("sendCommand", "Sortir de la maison", "sortirmaison"),
        ("sendCommand", "Joueurs connectés", "who"),
        ("sendCommand", "Sauvegarder le monde", "save world"),
        ("prepareCommand", "Recharger... (npcs, monsters, items, interactives)", "reload "),
    ]:
        e = item(kind, label, command)
        if kind == "separator":
            e.attrib.pop("label")
        populus.append(e)
    loadxml = [i for i, c in enumerate(root) if (c.get("type") or "").lower() == "loadxml"]
    root.insert(loadxml[0] if loadxml else len(root), populus)

    ET.indent(root, "\t")
    body = ET.tostring(root, encoding="unicode")
    with open(dst, "w", encoding="utf-8", newline="\r\n") as out:
        out.write('<?xml version="1.0" encoding="UTF-8" ?>\n')
        out.write("<!-- Menu admin converti pour le serveur Populus 2.51 : uniquement des commandes existantes. -->\n")
        out.write(body + "\n")

    kinds = Counter((e.get("type") or "").lower() for e in root.iter("item"))
    cmds = Counter((e.get("command") or "").split()[0] for e in root.iter("item") if e.get("command"))
    print("garde :", dict(kinds))
    print("commandes :", dict(cmds))
    print("retirés :")
    for k, v in stats.most_common():
        print(f"  {v:4} {k}")


if __name__ == "__main__":
    main(sys.argv[1], sys.argv[2])
