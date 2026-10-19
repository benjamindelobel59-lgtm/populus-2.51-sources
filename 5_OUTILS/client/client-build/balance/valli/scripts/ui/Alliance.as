package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Grid;
   import com.ankamagames.berilia.components.Input;
   import com.ankamagames.berilia.components.Label;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.berilia.types.graphic.UiRootContainer;
   import com.ankamagames.dofus.datacenter.guild.EmblemSymbol;
   import com.ankamagames.dofus.uiApi.ChatApi;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.SocialApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import com.ankamagames.dofus.uiApi.TimeApi;
   import com.ankamagames.dofus.uiApi.UtilApi;
   import com.ankamagames.dofusModuleLibrary.enum.SoundEnum;
   import d2actions.*;
   import d2enums.ComponentHookList;
   import d2enums.PrismListenEnum;
   import d2enums.ProtocolConstantsEnum;
   import d2enums.ShortcutHookListEnum;
   import d2hooks.*;
   import flash.events.TimerEvent;
   import flash.text.TextFieldAutoSize;
   import flash.utils.Dictionary;
   import flash.utils.Timer;
   
   public class Alliance
   {
      
      private static const VIL_MARQUEUR:String = "§VIL§";
      
      private static const VIL_ONGLET:int = 4;
      
      public var sysApi:SystemApi;
      
      public var uiApi:UiApi;
      
      public var utilApi:UtilApi;
      
      [Module(name="Ankama_Common")]
      public var modCommon:Object;
      
      public var socialApi:SocialApi;
      
      public var dataApi:DataApi;
      
      public var timeApi:TimeApi;
      
      public var playerApi:PlayedCharacterApi;
      
      public var chatApi:ChatApi;
      
      private var _nCurrentTab:int = -1;
      
      private var _alliance:Object;
      
      private var _totalDares:uint = 0;
      
      public var mainCtr:GraphicContainer;
      
      public var lbl_name:Label;
      
      public var btn_availableDares:ButtonContainer;
      
      public var lbl_tag:Label;
      
      public var tx_emblemBack:Texture;
      
      public var tx_emblemUp:Texture;
      
      public var tx_bulletinWarning:Texture;
      
      public var btn_members:ButtonContainer;
      
      public var btn_bulletin:ButtonContainer;
      
      public var btn_areas:ButtonContainer;
      
      public var btn_fights:ButtonContainer;
      
      public var ctr_motd:GraphicContainer;
      
      public var lbl_motd:Label;
      
      public var btn_motdEdit:ButtonContainer;
      
      public var ctr_editMotd:GraphicContainer;
      
      public var inp_motd:Input;
      
      public var btn_motdValid:ButtonContainer;
      
      public var btn_motdExit:ButtonContainer;
      
      public var btn_village:ButtonContainer;
      
      public var ctr_village:GraphicContainer;
      
      public var lbl_vInfo:Label;
      
      public var gd_vDest:Grid;
      
      private var _vilLignes:Dictionary = new Dictionary(true);
      
      private var _vilRevLignes:Dictionary = new Dictionary(true);
      
      private var _vilGest:Boolean = false;
      
      public var lbl_vRec:Label;
      
      public var inp_vKamas:Input;
      
      public var gd_vRec:Grid;
      
      private var _vilRecLignes:Dictionary = new Dictionary(true);
      
      public var btn_vDeposer:ButtonContainer;
      
      public var btn_vRetirer:ButtonContainer;
      
      public var btn_vBrigandin:ButtonContainer;
      
      public var btn_vForeuse:ButtonContainer;
      
      public var btn_vRevendre:ButtonContainer;
      
      private var _vilTimer:Timer;
      
      private var _vilDepuis:Number = -1;
      
      private var _vilDonnees:Array;
      
      private var _vilCommande:String;
      
      public function Alliance()
      {
         super();
      }
      
      public function main(... args) : void
      {
         this.btn_members.soundId = SoundEnum.TAB;
         this.btn_bulletin.soundId = SoundEnum.TAB;
         this.btn_areas.soundId = SoundEnum.TAB;
         this.btn_fights.soundId = SoundEnum.TAB;
         this.btn_village.soundId = SoundEnum.TAB;
         this.uiApi.addComponentHook(this.btn_village,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_vDeposer,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_vRetirer,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_vBrigandin,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_vForeuse,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_vRevendre,ComponentHookList.ON_RELEASE);
         this.inp_vKamas.restrictChars = "0-9";
         this.ctr_village.visible = false;
         this._vilTimer = new Timer(300);
         this._vilTimer.addEventListener(TimerEvent.TIMER,this.vilLire);
         this.sysApi.addHook(AllianceUpdateInformations,this.onAllianceUpdateInformations);
         this.sysApi.addHook(AllianceMotd,this.onAllianceMotd);
         this.sysApi.addHook(AllianceBulletin,this.onAllianceBulletin);
         this.uiApi.addShortcutHook(ShortcutHookListEnum.VALID_UI,this.onShortcut);
         this.uiApi.addShortcutHook(ShortcutHookListEnum.CLOSE_UI,this.onShortcut);
         this.uiApi.addComponentHook(this.btn_members,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_bulletin,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_areas,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.btn_fights,ComponentHookList.ON_RELEASE);
         this.uiApi.addComponentHook(this.lbl_name,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.lbl_name,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.tx_emblemBack,ComponentHookList.ON_TEXTURE_READY);
         this.uiApi.addComponentHook(this.tx_emblemUp,ComponentHookList.ON_TEXTURE_READY);
         this.uiApi.addComponentHook(this.lbl_motd,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.lbl_motd,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.btn_motdEdit,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.btn_motdEdit,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.btn_motdValid,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.btn_motdValid,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.btn_motdExit,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.btn_motdExit,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.btn_availableDares,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.btn_availableDares,ComponentHookList.ON_ROLL_OUT);
         this.uiApi.addComponentHook(this.tx_bulletinWarning,ComponentHookList.ON_ROLL_OVER);
         this.uiApi.addComponentHook(this.tx_bulletinWarning,ComponentHookList.ON_ROLL_OUT);
         this.inp_motd.html = false;
         this.inp_motd.maxChars = ProtocolConstantsEnum.USER_MAX_MOTD_LEN;
         this.tx_emblemBack.dispatchMessages = true;
         this.tx_emblemUp.dispatchMessages = true;
         this.sysApi.sendAction(new PrismsListRegister("Alliance",PrismListenEnum.PRISM_LISTEN_MINE));
         this._alliance = this.socialApi.getAlliance();
         this.updateAllianceInfos();
         if(this.socialApi.hasGuildRight(this.playerApi.id(),"isBoss"))
         {
            this.btn_motdEdit.visible = true;
         }
         else
         {
            this.btn_motdEdit.visible = false;
         }
         this.lbl_motd.textfield.autoSize = TextFieldAutoSize.LEFT;
         this.lbl_motd.text = this._alliance.formattedMotd;
         var tempHeight:Number = Number(this.lbl_motd.textfield.height);
         this.lbl_motd.textfield.autoSize = TextFieldAutoSize.NONE;
         if(this.lbl_motd.textfield.numLines == 1)
         {
            this.lbl_motd.textfield.height = tempHeight + 2;
         }
         else
         {
            this.lbl_motd.resizeText();
         }
         this.inp_motd.text = this._alliance.motd;
         if(this._alliance.lastNotifiedTimestamp > this.sysApi.getData("allianceBulletinLastVisitTimestamp"))
         {
            this.tx_bulletinWarning.visible = true;
         }
         else
         {
            this.tx_bulletinWarning.visible = false;
         }
         this._totalDares = this.socialApi.getTotalAllianceDares();
         this.btn_availableDares.visible = this._totalDares > 0;
         this.openSelectedTab(args[0][0],args[0][1]);
      }
      
      public function unload() : void
      {
         if(this._vilTimer)
         {
            this._vilTimer.stop();
            this._vilTimer.removeEventListener(TimerEvent.TIMER,this.vilLire);
         }
         this.sysApi.sendAction(new PrismsListRegister("Alliance",PrismListenEnum.PRISM_LISTEN_NONE));
         this.uiApi.unloadUi("subAllianceUi");
      }
      
      private function updateAllianceInfos() : void
      {
         this.lbl_name.text = this._alliance.allianceName;
         this.lbl_tag.text = this.chatApi.getAllianceLink(this._alliance,"[" + this._alliance.allianceTag + "]");
         this.tx_emblemBack.uri = this._alliance.backEmblem.fullSizeIconUri;
         this.tx_emblemUp.uri = this._alliance.upEmblem.fullSizeIconUri;
      }
      
      private function openSelectedTab(tab:uint, params:Object = null) : void
      {
         if(this._nCurrentTab == tab)
         {
            return;
         }
         this.uiApi.unloadUi("subAllianceUi");
         if(tab == VIL_ONGLET)
         {
            this.ctr_village.visible = true;
            this.vilEnvoyer("data");
         }
         else
         {
            this.ctr_village.visible = false;
            this._vilTimer.stop();
            this.uiApi.loadUiInside(this.getUiNameByTab(tab),this.mainCtr,"subAllianceUi",params) as UiRootContainer;
         }
         this.getButtonByTab(tab).selected = true;
         this._nCurrentTab = tab;
         this.uiApi.getUi("socialBase").uiClass.setSubTab(this._nCurrentTab);
      }
      
      private function getUiNameByTab(tab:uint) : String
      {
         if(tab == 0)
         {
            return "allianceMembers";
         }
         if(tab == 1)
         {
            return "allianceAreas";
         }
         if(tab == 2)
         {
            return "allianceFights";
         }
         if(tab == 3)
         {
            return "socialBulletin";
         }
         return null;
      }
      
      private function getButtonByTab(tab:uint) : Object
      {
         if(tab == 0)
         {
            return this.btn_members;
         }
         if(tab == 1)
         {
            return this.btn_areas;
         }
         if(tab == 2)
         {
            return this.btn_fights;
         }
         if(tab == 3)
         {
            return this.btn_bulletin;
         }
         if(tab == VIL_ONGLET)
         {
            return this.btn_village;
         }
         return null;
      }
      
      private function switchMotdEditMode(editMode:Boolean) : void
      {
         if(editMode)
         {
            this.inp_motd.text = this._alliance.motd;
            this.inp_motd.focus();
            this.inp_motd.setSelection(8388607,8388607);
            this.ctr_editMotd.visible = true;
            this.ctr_motd.visible = false;
         }
         else
         {
            this.ctr_editMotd.visible = false;
            this.ctr_motd.visible = true;
         }
      }
      
      public function onTextureReady(target:Object) : void
      {
         var icon:EmblemSymbol = null;
         switch(target)
         {
            case this.tx_emblemBack:
               this.utilApi.changeColor(this.tx_emblemBack.getChildByName("back"),this._alliance.backEmblem.color,1);
               break;
            case this.tx_emblemUp:
               icon = this.dataApi.getEmblemSymbol(this._alliance.upEmblem.idEmblem);
               if(icon.colorizable)
               {
                  this.utilApi.changeColor(this.tx_emblemUp.getChildByName("up"),this._alliance.upEmblem.color,0);
               }
               else
               {
                  this.utilApi.changeColor(this.tx_emblemUp.getChildByName("up"),this._alliance.upEmblem.color,0,true);
               }
         }
      }
      
      private function onAllianceUpdateInformations() : void
      {
         this._alliance = this.socialApi.getAlliance();
         this.updateAllianceInfos();
      }
      
      private function onAllianceMotd() : void
      {
         this._alliance = this.socialApi.getAlliance();
         this.lbl_motd.textfield.autoSize = TextFieldAutoSize.LEFT;
         this.lbl_motd.text = this._alliance.formattedMotd;
         var tempHeight:Number = Number(this.lbl_motd.textfield.height);
         this.lbl_motd.textfield.autoSize = TextFieldAutoSize.NONE;
         if(this.lbl_motd.textfield.numLines == 1)
         {
            this.lbl_motd.textfield.height = tempHeight + 2;
         }
         else
         {
            this.lbl_motd.resizeText();
         }
         this.inp_motd.text = this._alliance.motd;
      }
      
      private function onAllianceBulletin() : void
      {
         if(this._nCurrentTab == 3)
         {
            return;
         }
         this._alliance = this.socialApi.getAlliance();
         if(this._alliance.lastNotifiedTimestamp > this.sysApi.getData("allianceBulletinLastVisitTimestamp"))
         {
            this.tx_bulletinWarning.visible = true;
         }
         else
         {
            this.tx_bulletinWarning.visible = false;
         }
      }
      
      public function selectWhichTabHintsToDisplay() : void
      {
         if(this._nCurrentTab == VIL_ONGLET || !this.uiApi.me().childUiRoot)
         {
            return;
         }
         this.uiApi.me().childUiRoot.uiClass.showTabHints();
      }
      
      private function vilEnvoyer(commande:String) : void
      {
         var messages:Array = this.chatApi.getMessagesByChannel(10);
         this._vilDepuis = messages && messages.length > 0 ? Number(messages[messages.length - 1].id) : -1;
         this.sysApi.sendAction(new AllianceMotdSetRequest(VIL_MARQUEUR + commande));
         this._vilTimer.reset();
         this._vilTimer.start();
      }
      
      private function vilLire(e:TimerEvent) : void
      {
         var messages:Array = this.chatApi.getMessagesByChannel(10);
         if(!messages)
         {
            return;
         }
         var i:int = messages.length - 1;
         while(i >= 0)
         {
            var s:Object = messages[i];
            if(Number(s.id) <= this._vilDepuis)
            {
               return;
            }
            var texte:String = String(s.msg);
            if(texte.indexOf(VIL_MARQUEUR) == 0)
            {
               this._vilDepuis = Number(s.id);
               this._vilTimer.stop();
               this.vilAfficher(texte.substr(VIL_MARQUEUR.length).split(";"));
               return;
            }
            i--;
         }
      }
      
      private function vilKamas(valeur:*) : String
      {
         return this.utilApi.kamasToString(Number(valeur),"") + " k";
      }
      
      private function vilAfficher(d:Array) : void
      {
         if(d.length < 10)
         {
            return;
         }
         this._vilDonnees = d;
         var type:int = int(d[1]);
         var gest:Boolean = d[5] == "1";
         var chef:Boolean = d[6] == "1";
         var fenetre:Boolean = d[4] == "1";
         var nomPnj:String = type > 0 ? "Téléporteur de l\'alliance" : "aucun";
         var info:String = "Coffre de l\'alliance : " + this.vilKamas(d[0]) + "\n";
         info += "PNJ : " + nomPnj + (type > 0 ? " (village : " + d[2] + ")" : " — installez-le en étant dans un de vos villages de conquête") + "\n";
         info += "Prochaine destination : " + this.vilKamas(d[3]) + " (+25 % par destination déjà possédée)\n";
         info += fenetre ? "Revente possible maintenant : " + this.vilKamas(d[9]) + " (15 %, définitive)" : "Revente : seulement entre le début d\'une attaque du village et le début du KOH";
         this.lbl_vInfo.text = info;
         this._vilGest = gest;
         var lignes:Array = [];
         var dests:Array = d[7] == "" ? [] : String(d[7]).split("|");
         for each(var x:String in dests)
         {
            var f:Array = x.split("^");
            lignes.push({
               "id":int(f[0]),
               "reseau":(f[1] == "3" ? "Diligence" : (f[1] == "2" ? "Foreuse" : "Brigandin")),
               "nom":String(f[2]),
               "etat":String(f[3]),
               "revente":(f.length > 4 ? Number(f[4]) : -1)
            });
         }
         this.gd_vDest.dataProvider = lignes;
         var recs:Array = d[8] == "" ? [] : String(d[8]).split("|");
         var lr:Array = [];
         for each(var r:String in recs)
         {
            var g:Array = r.split("^");
            lr.push({
               "conquete":String(g[0]),
               "zone":String(g[1]),
               "dest":String(g[2]),
               "nom":String(g[3]),
               "possedee":g[4] == "1",
               "kamas":Number(g[5])
            });
         }
         this.gd_vRec.dataProvider = lr;
         this.gd_vRec.visible = lr.length > 0;
         this.lbl_vRec.text = lr.length > 0 ? (gest ? "Récompense de conquête : choisissez UNE destination (une seule par conquête, sous 48 h)" : "Récompense de conquête en attente (choix réservé aux gestionnaires des prismes)") : "Aucune récompense de conquête en attente.";
         this.btn_vRetirer.visible = chef;
         this.btn_vBrigandin.visible = gest && type == 0;
         this.btn_vForeuse.visible = false;
         this.btn_vRevendre.visible = gest && fenetre;
      }
      
      public function updateVillageLine(data:*, components:*, selected:Boolean) : void
      {
         if(!data)
         {
            components.lbl_vNom.text = "";
            components.lbl_vEtat.text = "";
            components.btn_vLigne.visible = false;
            components.btn_vRevLigne.visible = false;
            return;
         }
         if(!this._vilRevLignes[components.btn_vRevLigne.name])
         {
            this.uiApi.addComponentHook(components.btn_vRevLigne,ComponentHookList.ON_RELEASE);
         }
         this._vilRevLignes[components.btn_vRevLigne.name] = data;
         if(!this._vilLignes[components.btn_vLigne.name])
         {
            this.uiApi.addComponentHook(components.btn_vLigne,ComponentHookList.ON_RELEASE);
         }
         this._vilLignes[components.btn_vLigne.name] = data;
         components.lbl_vNom.text = "[" + data.reseau + "] " + data.nom;
         components.lbl_vEtat.text = data.etat == "1" ? "Active" : (data.etat == "2" ? "En réserve" : "");
         components.btn_vLigne.visible = data.etat == "0" && this._vilGest && this._vilDonnees && int(this._vilDonnees[1]) > 0;
         components.btn_vRevLigne.visible = data.etat != "0" && this._vilGest && data.revente >= 0;
      }
      
      public function updateRecompenseLine(data:*, components:*, selected:Boolean) : void
      {
         if(!data)
         {
            components.lbl_rTexte.text = "";
            components.btn_rChoisir.visible = false;
            return;
         }
         if(!this._vilRecLignes[components.btn_rChoisir.name])
         {
            this.uiApi.addComponentHook(components.btn_rChoisir,ComponentHookList.ON_RELEASE);
         }
         this._vilRecLignes[components.btn_rChoisir.name] = data;
         components.lbl_rTexte.text = data.zone + " — " + data.nom + " : " + (data.possedee ? "déjà possédée, " + this.vilKamas(data.kamas) + " au coffre" : "débloquée gratuitement");
         components.btn_rChoisir.visible = this._vilGest;
      }
      
      private function vilConfirmer(texte:String, commande:String) : void
      {
         this._vilCommande = commande;
         this.modCommon.openPopup("Village",texte,["Oui","Non"],[this.vilValider,null],this.vilValider);
      }
      
      private function vilValider() : void
      {
         if(this._vilCommande)
         {
            this.vilEnvoyer(this._vilCommande);
            this._vilCommande = null;
         }
      }
      
      private function vilClic(target:Object) : Boolean
      {
         var n:Number = Number(NaN);
         var ligne:Object = target && target.name ? this._vilLignes[target.name] : null;
         var recompense:Object = target && target.name ? this._vilRecLignes[target.name] : null;
         if(recompense)
         {
            this.vilConfirmer("Prendre « " + recompense.nom + " » (" + recompense.zone + ") : " + (recompense.possedee ? "déjà possédée, " + this.vilKamas(recompense.kamas) + " au coffre" : "débloquée gratuitement") + " ? Une seule récompense par conquête.","recompense " + recompense.conquete + " " + recompense.dest);
            return true;
         }
         var revente:Object = target && target.name ? this._vilRevLignes[target.name] : null;
         if(revente)
         {
            this.vilConfirmer("Revendre « " + revente.nom + " » pour " + this.vilKamas(revente.revente) + " (15 % du prix payé) ? C\'est définitif.","revendre1 " + revente.id);
            return true;
         }
         if(ligne)
         {
            this.vilConfirmer("Débloquer « " + ligne.nom + " » pour " + this.vilKamas(this._vilDonnees[3]) + " (payé par le coffre) ?","acheter " + ligne.id);
            return true;
         }
         switch(target)
         {
            case this.btn_village:
               this.openSelectedTab(VIL_ONGLET);
               return true;
            case this.btn_vDeposer:
               n = Number(this.inp_vKamas.text);
               if(n > 0)
               {
                  this.vilConfirmer("Déposer " + this.vilKamas(n) + " dans le coffre de l\'alliance ?","deposer " + n);
               }
               return true;
            case this.btn_vRetirer:
               n = Number(this.inp_vKamas.text);
               if(n > 0)
               {
                  this.vilConfirmer("Retirer " + this.vilKamas(n) + " du coffre de l\'alliance ?","retirer " + n);
               }
               return true;
            case this.btn_vBrigandin:
               this.vilConfirmer("Installer le Téléporteur de l\'alliance dans le village où vous êtes (payé par le coffre) ? Il ne débloque aucune destination : toutes s\'achètent ensuite (brigandins, foreuses, diligences).","pnj 1");
               return true;
            case this.btn_vForeuse:
               this.vilConfirmer("Installer le PNJ de l\'alliance (apparence Foreuse) dans le village où vous êtes (payé par le coffre) ? Il ne débloque aucune destination : toutes s\'achètent ensuite (brigandins, foreuses, diligences).","pnj 2");
               return true;
            case this.btn_vRevendre:
               if(this._vilDonnees)
               {
                  this.vilConfirmer("Revendre toutes les destinations du village pour " + this.vilKamas(this._vilDonnees[9]) + " ? C\'est DÉFINITIF, même si vous gagnez la défense.","revendre");
               }
               return true;
            default:
               return false;
         }
      }
      
      public function onRelease(target:Object) : void
      {
         if(this.vilClic(target))
         {
            return;
         }
         if(target == this.btn_members)
         {
            this.openSelectedTab(0);
         }
         else if(target == this.btn_bulletin)
         {
            this.tx_bulletinWarning.visible = false;
            this.openSelectedTab(3);
         }
         else if(target == this.btn_areas)
         {
            this.openSelectedTab(1);
         }
         else if(target == this.btn_fights)
         {
            this.openSelectedTab(2);
         }
         else if(target == this.btn_motdEdit)
         {
            this.switchMotdEditMode(true);
         }
         else if(target == this.btn_motdExit)
         {
            this.switchMotdEditMode(false);
         }
         else if(target == this.btn_motdValid)
         {
            if(this.inp_motd.text != this._alliance.motd)
            {
               this.sysApi.sendAction(new AllianceMotdSetRequest(this.inp_motd.text));
            }
            this.switchMotdEditMode(false);
         }
         else if(target == this.btn_availableDares)
         {
            this.sysApi.dispatchHook(OpenDareSearch,this._alliance.allianceName,"searchFilterAlliance");
         }
      }
      
      public function onRollOver(target:Object) : void
      {
         var tooltipText:String = null;
         var date:Number = Number(NaN);
         var point:uint = 7;
         var relPoint:uint = 1;
         switch(target)
         {
            case this.lbl_name:
               tooltipText = this.uiApi.getText("ui.common.creationDate") + this.uiApi.getText("ui.common.colon") + this.timeApi.getDate(this._alliance.creationDate * 1000) + " " + this.timeApi.getClock(this._alliance.creationDate * 1000);
               break;
            case this.btn_motdEdit:
               tooltipText = this.uiApi.getText("ui.motd.edit");
               break;
            case this.btn_motdValid:
               tooltipText = this.uiApi.getText("ui.common.validation");
               break;
            case this.btn_motdExit:
               tooltipText = this.uiApi.getText("ui.common.cancel");
               break;
            case this.tx_bulletinWarning:
               tooltipText = this.uiApi.getText("ui.motd.bulletinUpdated");
               break;
            case this.lbl_motd:
               tooltipText = "";
               if(this.lbl_motd.hasTooltipExtension)
               {
                  tooltipText += this.lbl_motd.text;
               }
               if(this._alliance.motdWriterName != "")
               {
                  if(tooltipText != "")
                  {
                     tooltipText += "\n";
                  }
                  date = this._alliance.motdTimestamp * 1000;
                  tooltipText += this.uiApi.getText("ui.motd.lastModification",this.timeApi.getDate(date,true) + " " + this.timeApi.getClock(date,true,true),this._alliance.motdWriterName);
               }
               break;
            case this.btn_availableDares:
               tooltipText = this.uiApi.processText(this.uiApi.getText("ui.social.dare.totalAvailable",this._totalDares),"",this._totalDares == 1);
         }
         if(tooltipText)
         {
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(tooltipText),target,false,"standard",point,relPoint,3,null,null,null,"TextInfo");
         }
      }
      
      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
      }
      
      public function onShortcut(s:String) : Boolean
      {
         switch(s)
         {
            case "validUi":
               if(this.inp_motd.visible && this.inp_motd.haveFocus)
               {
                  if(this.inp_motd.text != this._alliance.motd)
                  {
                     this.sysApi.sendAction(new AllianceMotdSetRequest(this.inp_motd.text));
                  }
                  this.switchMotdEditMode(false);
                  return true;
               }
               return false;
               break;
            case "closeUi":
               if(this.inp_motd.visible && this.inp_motd.haveFocus)
               {
                  this.switchMotdEditMode(false);
                  return true;
               }
               this.uiApi.unloadUi("socialBase");
               return true;
               break;
            default:
               return false;
         }
      }
   }
}

