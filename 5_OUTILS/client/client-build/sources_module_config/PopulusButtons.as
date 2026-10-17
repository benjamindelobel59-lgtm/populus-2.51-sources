package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import d2hooks.TextInformation;
   import com.ankamagames.dofus.uiApi.FightApi;
   import flash.events.TimerEvent;
   import flash.utils.Timer;
   import flash.display.Stage;
   import flash.net.SharedObject;
   import populus.ChatTextOutput;
   import populus.PanelWindow;

   // Populus / Mystic : barre d'icones Groupe / One Window / IP Drop / Sasa auto / Passe-tour auto.
   // Cadre gris = desactive, vert = active. L'etat vient des messages du serveur
   // (« One Window active / desactive », « IP Drop ... », « Sasa auto ... », « Passe-tour auto ... »).
   public class PopulusButtons
   {

      private static const GRIS:uint = 5921370;

      private static const VERT:uint = 3059774;

      private static const OR:uint = 13148746;

      // nom du bouton, commande, debut du message serveur, infobulle
      private static const BOUTONS:Array = [["groupe",".groupe",null,"Groupe : invite tous vos personnages"],["onewindow",".onewindow","One Window","One Window : vos personnages vous suivent et vous jouez leurs tours"],["ipdrop",".ipdrop","IP Drop","IP Drop : ce personnage reçoit le drop de tous vos personnages"],["sasa",".sasa","Sasa auto","Sasa auto : les points de caractéristique vont dans la sagesse"],["pass",".passturn","Passe-tour auto","Passe-tour auto : les tours de ce personnage sont passés automatiquement"],["invo",".invo","Contrôle invocations","Contrôle des invocations : vous jouez vous-même vos invocations"],["bug",".bug",null,"Signaler un bug : titre, description, logs du combat et du client"]];

      private var _store:SharedObject;

      public var uiApi:UiApi;

      private var _hidden:Boolean;

      public var sysApi:SystemApi;

      // Mystic : horloge de l'Oeil de Vortex (etat « Xe heure » 221..232 de l'Auroraire 3833), affichee en haut de l'ecran
      public var fightApi:FightApi;

      public var ctr_horloge:*;

      public var lbl_horloge:*;

      private static const AURORAIRE:int = 3833;

      private var _minuteur:Timer;

      private static const HEURES:Array = ["","1re","2e","3e","4e","5e","6e","7e","8e","9e","10e","11e","12e"];

      public var ctr_h:*;

      private var _vertical:Boolean;

      public var ctr_v:*;

      // controle des invocations : active par defaut cote serveur → vert au demarrage
      private var _etat:Object = {"invo":true};

      public var btn_groupe:*;

      public var btn_onewindow:*;

      public var btn_ipdrop:*;

      public var btn_sasa:*;

      public var btn_pass:*;
      
      public var btn_invo:*;

      public var btn_bug:*;

      public var btn_groupe2:*;

      public var btn_onewindow2:*;

      public var btn_ipdrop2:*;

      public var btn_sasa2:*;

      public var btn_pass2:*;
      
      public var btn_invo2:*;

      public var btn_bug2:*;

      public var f_groupe:*;

      public var f_onewindow:*;

      public var f_ipdrop:*;

      public var f_sasa:*;

      public var f_pass:*;
      
      public var f_invo:*;

      public var f_bug:*;

      public var f_groupe2:*;

      public var f_onewindow2:*;

      public var f_ipdrop2:*;

      public var f_sasa2:*;

      public var f_pass2:*;
      
      public var f_invo2:*;

      public var f_bug2:*;

      public function PopulusButtons()
      {
         super();
      }

      public function onTextInformation(param1:String = null, param2:* = null, param3:* = null, param4:* = null) : void
      {
         var _loc6_:* = undefined;
         var _loc7_:Array = null;
         var _loc8_:int = 0;
         var _loc9_:String = null;
         if(!param1)
         {
            return;
         }
         var _loc5_:Array = param1.match(/(https?:\/\/[^\s,{}]+\/populus-admin\/\?t=[a-f0-9]{48})/);
         if(_loc5_)
         {
            _loc6_ = this.ctr_h || this.ctr_v;
            PanelWindow.open(_loc6_ ? _loc6_.stage : null,_loc5_[1]);
         }
         // Mystic : « Signaler un bug » : formulaire du panel (bug.php) + envoi de la fin du log du client
         var _bug:Array = param1.match(/(https?:\/\/[^\s,{}]+\/populus-admin\/bug\.php\?t=[a-f0-9]{48})/);
         if(_bug)
         {
            _loc6_ = this.ctr_h || this.ctr_v;
            PanelWindow.open(_loc6_ ? _loc6_.stage : null,_bug[1]);
            PanelWindow.envoyerLog(_bug[1] + "&log=1");
         }
         // etat des boutons : « <nom> active » -> vert, « <nom> desactive » -> gris
         for each(_loc7_ in BOUTONS)
         {
            if(_loc7_[2])
            {
               _loc8_ = param1.indexOf(_loc7_[2] + " ");
               if(_loc8_ >= 0)
               {
                  _loc9_ = param1.substr(_loc8_ + String(_loc7_[2]).length + 1,5);
                  if(_loc9_ == "activ")
                  {
                     this._etat[_loc7_[0]] = true;
                  }
                  else if(_loc9_.charAt(0) == "d")
                  {
                     this._etat[_loc7_[0]] = false;
                  }
               }
            }
         }
         this.peindre();
      }

      private function peindre() : void
      {
         var _loc1_:Array = null;
         var _loc2_:uint = 0;
         var _loc3_:* = undefined;
         for each(_loc1_ in BOUTONS)
         {
            _loc2_ = !_loc1_[2] ? OR : (this._etat[_loc1_[0]] ? VERT : GRIS);
            for each(_loc3_ in [this["f_" + _loc1_[0]],this["f_" + _loc1_[0] + "2"]])
            {
               if(_loc3_)
               {
                  try
                  {
                     _loc3_.bgColor = _loc2_;
                  }
                  catch(e:Error)
                  {
                  }
               }
            }
         }
      }

      private function bouton(param1:String) : Array
      {
         var _loc2_:Array = null;
         for each(_loc2_ in BOUTONS)
         {
            if(param1 == "btn_" + _loc2_[0] || param1 == "btn_" + _loc2_[0] + "2")
            {
               return _loc2_;
            }
         }
         return null;
      }

      public function onRelease(param1:Object) : void
      {
         var _loc2_:String = param1 ? String(param1.name) : "";
         var _loc3_:Array = this.bouton(_loc2_);
         if(_loc3_)
         {
            this.send(_loc3_[1]);
         }
         else if(_loc2_.indexOf("btn_rotate") == 0)
         {
            this._vertical = !this._vertical;
            this.save();
            this.apply();
         }
      }

      public function onRollOver(param1:Object) : void
      {
         var _loc2_:Array = this.bouton(param1 ? String(param1.name) : "");
         if(!_loc2_)
         {
            return;
         }
         var _loc3_:String = _loc2_[3];
         if(_loc2_[2])
         {
            _loc3_ += this._etat[_loc2_[0]] ? " (activé)" : " (désactivé)";
         }
         try
         {
            this.uiApi.showTooltip(this.uiApi.textTooltipInfo(_loc3_),param1,false,"standard",7,1,3,null,null,null,"TextInfo");
         }
         catch(e:Error)
         {
         }
      }

      public function onRollOut(param1:Object) : void
      {
         try
         {
            this.uiApi.hideTooltip();
         }
         catch(e:Error)
         {
         }
      }

      public function toggleBar() : void
      {
         this._hidden = !this._hidden;
         this.save();
         this.apply();
      }

      private function send(param1:String) : void
      {
         this.sysApi.sendAction(new ChatTextOutput(param1,0,"",null));
      }

      private function save() : void
      {
         if(!this._store)
         {
            return;
         }
         this._store.data.vertical = this._vertical;
         this._store.data.hidden = this._hidden;
         try
         {
            this._store.flush();
         }
         catch(e:Error)
         {
         }
      }

      public function main(param1:* = null) : void
      {
         var _loc2_:Array = null;
         var _loc3_:* = undefined;
         var params:* = param1;
         try
         {
            this._store = SharedObject.getLocal("populusButtons");
            this._vertical = this._store.data.vertical == true;
            this._hidden = this._store.data.hidden == true;
         }
         catch(e:Error)
         {
            this._store = null;
         }
         this.apply();
         this.peindre();
         for each(_loc2_ in BOUTONS)
         {
            for each(_loc3_ in [this["btn_" + _loc2_[0]],this["btn_" + _loc2_[0] + "2"]])
            {
               if(_loc3_)
               {
                  try
                  {
                     this.uiApi.addComponentHook(_loc3_,"onRollOver");
                     this.uiApi.addComponentHook(_loc3_,"onRollOut");
                  }
                  catch(e:Error)
                  {
                  }
               }
            }
         }
         try
         {
            this.sysApi.addHook(TextInformation,this.onTextInformation);
         }
         catch(e:Error)
         {
         }
         // horloge relue toutes les 500 ms (pas de nouveau hook : le module ne contient que ceux qu'il utilisait deja)
         try
         {
            this._minuteur = new Timer(500);
            this._minuteur.addEventListener(TimerEvent.TIMER,this.onMinuteur);
            this._minuteur.start();
         }
         catch(e:Error)
         {
         }
         this.horloge();
      }

      public function onMinuteur(param1:* = null) : void
      {
         this.horloge();
      }

      // cherche l'Auroraire dans le combat et son etat d'heure ; rien trouve -> bandeau cache
      private function horloge() : void
      {
         var heure:int = 0;
         var id:* = undefined;
         var buff:* = undefined;
         var etat:int = 0;
         try
         {
            if(this.fightApi)
            {
               for each(id in this.fightApi.getFighters())
               {
                  if(this.fightApi.getMonsterId(id) == AURORAIRE)
                  {
                     for each(buff in this.fightApi.getBuffList(id))
                     {
                        etat = 0;
                        try
                        {
                           etat = int(buff.stateId);
                        }
                        catch(e2:Error)
                        {
                        }
                        if(etat >= 221 && etat <= 232)
                        {
                           heure = etat - 220;
                        }
                     }
                  }
               }
            }
         }
         catch(e:Error)
         {
            heure = 0;
         }
         if(!this.ctr_horloge)
         {
            return;
         }
         this.ctr_horloge.visible = heure > 0;
         if(heure > 0 && this.lbl_horloge)
         {
            this.lbl_horloge.text = "Auroraire : " + HEURES[heure] + " heure";
         }
      }

      private function apply() : void
      {
         if(this.ctr_h)
         {
            this.ctr_h.visible = !this._hidden && !this._vertical;
         }
         if(this.ctr_v)
         {
            this.ctr_v.visible = !this._hidden && this._vertical;
         }
      }

      public function unload() : void
      {
         if(this._minuteur)
         {
            this._minuteur.stop();
            this._minuteur.removeEventListener(TimerEvent.TIMER,this.onMinuteur);
            this._minuteur = null;
         }
      }
   }
}
