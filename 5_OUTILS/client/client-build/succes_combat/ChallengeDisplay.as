package ui
{
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.berilia.components.Texture;
   import com.ankamagames.berilia.components.TextureBitmap;
   import com.ankamagames.berilia.types.graphic.ButtonContainer;
   import com.ankamagames.berilia.types.graphic.GraphicContainer;
   import com.ankamagames.dofus.uiApi.DataApi;
   import com.ankamagames.dofus.uiApi.PlayedCharacterApi;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import d2actions.*;
   import d2hooks.*;

   // MYSTIC 17/10 : deux colonnes, chacune avec son bouton de repli : les challenges (avec bonus) et les succès de boss (envoyés par le
   // serveur comme des challenges sans bonus : xpBonus = dropBonus = 0).
   public class ChallengeDisplay
   {

      public var sysApi:SystemApi;

      public var uiApi:UiApi;

      public var dataApi:DataApi;

      public var playerApi:PlayedCharacterApi;

      public var ctr_challenges:GraphicContainer;

      public var btn_minimize:ButtonContainer;

      public var tx_background_grid:TextureBitmap;

      public var tx_button_minimize:Texture;

      public var ctr_succes:GraphicContainer;

      public var ctr_succesBloc:GraphicContainer;

      public var btn_minimizeSucces:ButtonContainer;

      public var tx_background_succes:TextureBitmap;

      public var tx_button_minimizeSucces:Texture;

      private var _challenges:Array;

      private var _succes:Array;

      private var _challengeIconOffset:int;

      private var _ctrChallengeHeightModifier:int;

      private var _ctrChallengeHeight:int;

      public function ChallengeDisplay()
      {
         super();
      }

      public function main(oParam:Object = null) : void
      {
         if(!oParam)
         {
            oParam = {"challenges":[this.dataApi.getChallengeWrapper(1),this.dataApi.getChallengeWrapper(2)]};
         }
         this.sysApi.addHook(ChallengeInfoUpdate,this.onChallengeInfoUpdate);
         this.sysApi.addHook(FoldAll,this.onFoldAll);
         this._challengeIconOffset = this.uiApi.me().getConstant("challengeIconOffset");
         this._ctrChallengeHeightModifier = this.uiApi.me().getConstant("ctrChallengeHeightModifier");
         this._ctrChallengeHeight = this.uiApi.me().getConstant("ctrChallengeHeight");
         this._challenges = new Array();
         this._succes = new Array();
         this.updateChallengeList(oParam.challenges);
      }

      public function unload() : void
      {
      }

      private function estSucces(data:Object) : Boolean
      {
         return data != null && data.xpBonus == 0 && data.dropBonus == 0;
      }

      private function remplir(liste:Array, entrees:Array, conteneur:Object) : uint
      {
         var data:Object = null;
         var challenge:Object = null;
         var y:uint = uint(this._challengeIconOffset);
         var i:uint = 0;
         for each(data in liste)
         {
            if(entrees.length <= i)
            {
               challenge = new ChallengeEntry(this.uiApi.me(),conteneur,data,3,y);
               this.uiApi.addComponentHook(challenge.ctr_challenge,"onRollOver");
               this.uiApi.addComponentHook(challenge.ctr_challenge,"onRollOut");
               entrees.push(challenge);
            }
            else
            {
               entrees[i].data = data;
            }
            y += this._ctrChallengeHeightModifier;
            i++;
         }
         return y;
      }

      private function updateChallengeList(challenges:Object) : void
      {
         var data:Object = null;
         var normaux:Array = new Array();
         var succes:Array = new Array();
         for each(data in challenges)
         {
            if(this.estSucces(data))
            {
               succes.push(data);
            }
            else
            {
               normaux.push(data);
            }
         }
         this.tx_background_grid.height = this.remplir(normaux,this._challenges,this.ctr_challenges) + this._ctrChallengeHeight;
         this.tx_background_succes.height = this.remplir(succes,this._succes,this.ctr_succes) + this._ctrChallengeHeight;
         this.ctr_succesBloc.visible = succes.length > 0;
      }

      private function basculer(conteneur:Object, icone:Texture, forceHide:int) : void
      {
         var mustShow:Boolean = false;
         if(forceHide == 1)
         {
            mustShow = false;
         }
         else if(forceHide == 0)
         {
            mustShow = true;
         }
         else
         {
            mustShow = !conteneur.visible;
         }
         conteneur.visible = mustShow;
         icone.uri = this.uiApi.createUri(this.uiApi.me().getConstant("texture") + (mustShow ? "hud/icon_minus_floating_menu.png" : "hud/icon_plus_floating_menu.png"));
      }

      private function _showChallengeList(forceHide:int = -1) : void
      {
         this.basculer(this.ctr_challenges,this.tx_button_minimize,forceHide);
      }

      private function _showSuccesList(forceHide:int = -1) : void
      {
         this.basculer(this.ctr_succes,this.tx_button_minimizeSucces,forceHide);
      }

      private function getChallengeByButton(button:Object) : ChallengeEntry
      {
         var challenge:ChallengeEntry = null;
         for each(challenge in this._challenges)
         {
            if(challenge.ctr_challenge == button)
            {
               return challenge;
            }
         }
         for each(challenge in this._succes)
         {
            if(challenge.ctr_challenge == button)
            {
               return challenge;
            }
         }
         return null;
      }

      public function onRelease(target:Object) : void
      {
         switch(target)
         {
            case this.btn_minimize:
               this._showChallengeList();
               break;
            case this.btn_minimizeSucces:
               this._showSuccesList();
         }
      }

      private function onFoldAll(fold:Boolean) : void
      {
         this._showChallengeList(fold ? 1 : 0);
         this._showSuccesList(fold ? 1 : 0);
      }

      public function onRollOver(target:Object) : void
      {
         var challenge:ChallengeEntry = this.getChallengeByButton(target);
         if(Boolean(challenge) && Boolean(challenge.data))
         {
            this.uiApi.showTooltip(challenge.data,target,false,"standard",0,2,3,null,null,null);
            this.sysApi.sendAction(new ChallengeTargetsListRequest(challenge.data.id));
         }
      }

      public function onRollOut(target:Object) : void
      {
         this.uiApi.hideTooltip();
      }

      public function onChallengeInfoUpdate(challenges:Object) : void
      {
         this.updateChallengeList(challenges);
      }
   }
}
