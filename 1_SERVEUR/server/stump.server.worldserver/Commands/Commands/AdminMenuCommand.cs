using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Commands;
using Stump.Server.WorldServer.Commands.Trigger;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using Stump.Server.WorldServer.Handlers.Chat;

namespace Stump.Server.WorldServer.Commands.Commands
{
    /// <summary>
    /// Clickable admin menu shown in the chat, read from menuadmin.xml (same format as the Ankama admin menu).
    /// Each line is a client hyperlink {admin,true,base64(command)::label} : clicking it sends the command
    /// through the admin console (only for accounts with admin rights).
    ///   .menu          first level
    ///   .menu 3.2      sub-menu at index 3 then 2
    /// </summary>
    public class AdminMenuCommand : CommandBase
    {
        const string MenuFile = "menuadmin.xml";

        static XElement m_root;
        static DateTime m_loadedAt;

        public AdminMenuCommand()
        {
            Aliases = new[] { "menu" };
            Description = "Menu admin cliquable (menuadmin.xml)";
            RequiredRole = RoleEnum.Administrator;
            AddParameter("path", "p", "Chemin du sous-menu (ex: 3.2)", string.Empty, true);
            AddParameter("page", "pg", "Page", 0, true);
        }

        public override void Execute(TriggerBase trigger)
        {
            var character = (trigger as GameTrigger)?.Character;
            if (character == null)
                return;

            var root = LoadMenu();
            if (root == null)
            {
                Send(character, $"Fichier {MenuFile} introuvable à côté du WorldServer.");
                return;
            }

            var path = trigger.Get<string>("path");
            var node = root;
            var titles = new List<string>();
            foreach (var part in path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var children = Children(node).ToList();
                if (!int.TryParse(part, out var index) || index < 0 || index >= children.Count)
                {
                    Send(character, "Chemin de menu invalide.");
                    return;
                }
                node = children[index];
                titles.Add((string)node.Attribute("label"));
            }

            var sb = new StringBuilder();
            sb.Append("<b>Menu admin").Append(titles.Count > 0 ? Html(" > " + string.Join(" > ", titles)) : "").Append("</b>");

            if (titles.Count > 0)
            {
                var parent = string.Join(".", path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Reverse().Skip(1).Reverse());
                sb.Append("\n").Append(Link("menu " + parent, "« Retour"));
            }

            const int pageSize = 40;
            var items = Children(node).ToList();
            var page = Math.Max(0, trigger.Get<int>("page"));
            var first = page * pageSize;
            for (var i = first; i < items.Count && i < first + pageSize; i++)
            {
                var item = items[i];
                var type = (string)item.Attribute("type");
                var label = (string)item.Attribute("label") ?? "";
                var childPath = string.IsNullOrEmpty(path) ? i.ToString() : path + "." + i;

                switch (type)
                {
                    case "menu":
                        sb.Append("\n").Append(Link("menu " + childPath, "[+] " + label));
                        break;
                    case "static":
                        if (!titles.Contains(label))
                            sb.Append("\n<i>").Append(Html(label.Replace("%p", character.Name))).Append("</i>");
                        break;
                    case "sendCommand":
                    case "batch":
                        var commands = (type == "batch" ? item.Elements("item") : new[] { item })
                            .Select(x => Translate((string)x.Attribute("command"), character))
                            .Where(x => x != null)
                            .ToArray();
                        if (commands.Length > 0)
                            sb.Append("\n").Append(Link(string.Join(" ; ", commands), "> " + label));
                        break;
                }
            }

            if (items.Count > first + pageSize)
                sb.Append("\n").Append(Link($"menu {(string.IsNullOrEmpty(path) ? "." : path)} {page + 1}", $"Page suivante ({page + 2}/{(items.Count + pageSize - 1) / pageSize})"));

            Send(character, sb.ToString());
        }

        static IEnumerable<XElement> Children(XElement node)
            => node.Elements("item").Where(x => (string)x.Attribute("type") != null);

        static XElement LoadMenu()
        {
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MenuFile);
            if (!File.Exists(file))
                return null;

            var modified = File.GetLastWriteTime(file);
            if (m_root == null || modified != m_loadedAt)
            {
                // UTF-8 ; the few Latin-1 bytes of the Mustam file are in its comments, removed below.
                var text = Encoding.UTF8.GetString(File.ReadAllBytes(file));
                // The Flash parser is lenient : drop the declaration and the comments ("--" inside them is invalid XML)
                // and escape lone '&'.
                text = Regex.Replace(text.TrimStart('﻿'), @"^\s*<\?xml[^>]*\?>", "");
                text = Regex.Replace(text, "<!--.*?-->", "", RegexOptions.Singleline);
                text = Regex.Replace(text, @"&(?!(amp|lt|gt|quot|apos|#\d+|#x[0-9a-fA-F]+);)", "&amp;");
                m_root = XDocument.Parse(text).Root;
                m_loadedAt = modified;
            }
            return m_root;
        }

        /// <summary>
        /// Converts the Ankama/Mustam admin commands to this server's commands. Returns null when the command
        /// only makes sense for the client (console management, menu reload).
        /// </summary>
        static string Translate(string command, Character character)
        {
            if (string.IsNullOrWhiteSpace(command))
                return null;

            command = command.Trim()
                .Replace("%n", character.Name)
                .Replace("%m", character.Map.Id.ToString())
                .Replace("%p", character.Name);

            var move = Regex.Match(command, @"^move\s+\S+\s+(\d+)(\s+(\d+))?$");
            if (move.Success)
                return "go " + move.Groups[1].Value;

            var item = Regex.Match(command, @"^item\s+\S+\s+(\d+)\s+(\d+)");
            if (item.Success)
                return $"item add {item.Groups[1].Value} {character.Name} {item.Groups[2].Value}";

            return command.Replace(" * ", " " + character.Name + " ");
        }

        static string Link(string command, string label)
        {
            var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(command));
            return "{admin,true," + encoded + "::" + label.Replace("}", ")").Replace("{", "(").Replace("<", "«") + "}";
        }

        // The chat is rendered as HTML : a raw '<' cuts the rest of the message.
        static string Html(string text)
            => text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        static void Send(Character character, string text)
            => ChatHandler.SendChatServerMessage(character.Client, text);
    }
}
