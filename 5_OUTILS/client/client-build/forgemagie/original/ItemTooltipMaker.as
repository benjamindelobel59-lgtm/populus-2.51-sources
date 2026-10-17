package makers
{
   import blockParams.EffectsTooltipBlockParameters;
   import blocks.ConditionTooltipBlock;
   import blocks.DescriptionTooltipBlock;
   import blocks.HtmlEffectsTooltipBlock;
   import blocks.ItemDetailsBlock;
   import blocks.ItemHeaderBlock;
   import blocks.TextTooltipBlock;
   import blocks.TextWithTwoColumnsBlock;
   import com.ankamagames.berilia.api.UiApi;
   import com.ankamagames.dofus.datacenter.effects.EffectInstance;
   import com.ankamagames.dofus.datacenter.idols.Idol;
   import com.ankamagames.dofus.uiApi.SystemApi;
   import d2enums.DataEnum;
   import flash.ui.Keyboard;
   
   public class ItemTooltipMaker
   {
      
      private static const chunkType:String = "htmlChunks";
      
      private static const NAME_EFFECTS_IDS:Array = [985,988];
      
      private var _effects:Array;
      
      private var _itemData:*;
      
      public function ItemTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var eff:* = undefined;
         var effectsTooltipBlockParams:EffectsTooltipBlockParameters = null;
         var effect:* = undefined;
         var playerSetInfo:Object = null;
         var idol:Idol = null;
         var showTheoreticalEffectsTip:String = null;
         var leftText:String = null;
         var uiApi:UiApi = Api.ui;
         var sysApi:SystemApi = Api.system;
         if(data.hasOwnProperty("itemWrapper"))
         {
            this._itemData = data.itemWrapper;
         }
         else
         {
            this._itemData = data;
         }
         var theoreticalEffects:* = Api.data.getItem(this._itemData.objectGID).possibleEffects;
         var setMode:Boolean = false;
         var abstractEffects:Array = [];
         var _param:paramClass = new paramClass();
         var baseChunkFile:String = chunkType + "/item/baseWithBackground.txt";
         if(param)
         {
            if(param.hasOwnProperty("noBg"))
            {
               _param.noBg = param.noBg;
               if(_param.noBg)
               {
                  baseChunkFile = chunkType + "/item/base.txt";
               }
            }
            if(param.hasOwnProperty("setMode"))
            {
               setMode = Boolean(param.setMode);
               delete param.setMode;
            }
            if(param.hasOwnProperty("description"))
            {
               _param.description = param.description;
            }
            if(param.hasOwnProperty("effects"))
            {
               _param.effects = param.effects;
            }
            if(param.hasOwnProperty("CC_EC"))
            {
               _param.CC_EC = param.CC_EC;
            }
            if(param.hasOwnProperty("noFooter"))
            {
               _param.noFooter = param.noFooter;
            }
            if(param.hasOwnProperty("noTheoreticalEffects"))
            {
               _param.noTheoreticalEffects = param.noTheoreticalEffects;
            }
            if(param.hasOwnProperty("header"))
            {
               _param.header = param.header;
               if(!_param.header)
               {
                  baseChunkFile = baseChunkFile.replace(".txt","NoIcon.txt");
               }
            }
            if(param.hasOwnProperty("conditions"))
            {
               _param.conditions = param.conditions;
            }
            if(param.hasOwnProperty("targetConditions"))
            {
               _param.targetConditions = param.targetConditions;
            }
            if(param.hasOwnProperty("length"))
            {
               _param.length = param.length;
            }
            if(param.hasOwnProperty("equipped"))
            {
               _param.equipped = param.equipped;
            }
            if(param.hasOwnProperty("shortcutKey"))
            {
               _param.shortcutKey = param.shortcutKey;
            }
            if(Boolean(param.hasOwnProperty("showEffects")) && Boolean(param.showEffects) && Boolean(this._itemData) && this._itemData.effects.length <= 0)
            {
               for each(effect in theoreticalEffects)
               {
                  abstractEffects.push(effect);
               }
            }
            if(param.hasOwnProperty("averagePrice"))
            {
               _param.averagePrice = param.averagePrice;
            }
            if(param.hasOwnProperty("contextual"))
            {
               _param.contextual = param.contextual;
            }
            if(param.hasOwnProperty("addTheoreticalEffects"))
            {
               _param.addTheoreticalEffects = param.addTheoreticalEffects;
            }
            else
            {
               param.addTheoreticalEffects = false;
            }
            if(param.hasOwnProperty("showDropPercentage"))
            {
               _param.showDropPercentage = param.showDropPercentage;
            }
         }
         var tooltip:Object = Api.tooltip.createTooltip(baseChunkFile,chunkType + "/base/container.txt",chunkType + "/base/separator.txt");
         tooltip.chunkType = chunkType;
         if(_param.equipped)
         {
            tooltip.addBlock(new TextTooltipBlock(uiApi.getText("ui.item.equipped"),{
               "css":"[local.css]normal.css",
               "classCss":"disabled"
            },chunkType).block);
         }
         tooltip.addBlock(new ItemHeaderBlock(this._itemData,_param,chunkType).block);
         this._effects = [];
         var showTheoretical:Boolean = false;
         if(abstractEffects.length)
         {
            for each(eff in abstractEffects)
            {
               this._effects.push(eff);
            }
            showTheoretical = true;
         }
         else
         {
            for each(eff in this._itemData.effects)
            {
               this._effects.push(eff);
            }
         }
         if(Boolean(this._itemData.enhanceable) && Boolean(!_param.addTheoreticalEffects) && (Boolean(sysApi.isKeyDown(Keyboard.CONTROL)) || Boolean(sysApi.getOption("alwaysDisplayTheoreticalEffectsInTooltip","dofus"))) && !showTheoretical)
         {
            if(!_param.noTheoreticalEffects)
            {
               if(param)
               {
                  param.addTheoreticalEffects = true;
               }
               _param.addTheoreticalEffects = true;
            }
         }
         else if(!this._itemData.enhanceable)
         {
            theoreticalEffects = null;
            _param.addTheoreticalEffects = false;
         }
         for each(eff in this._itemData.favoriteEffect)
         {
            this._effects.push(eff);
         }
         if(Boolean(this._effects.length) && _param.effects)
         {
            this._effects.sort(this.compareEffectsOrder);
            effectsTooltipBlockParams = EffectsTooltipBlockParameters.create(this._effects,chunkType);
            effectsTooltipBlockParams.length = _param.length;
            effectsTooltipBlockParams.showTheoreticalEffects = showTheoretical;
            effectsTooltipBlockParams.addTheoreticalEffects = _param.addTheoreticalEffects;
            effectsTooltipBlockParams.itemTheoreticalEffects = theoreticalEffects;
            if(Boolean(param.hasOwnProperty("effects")) && Boolean(param.effects) || Boolean(param.hasOwnProperty("damages")) && Boolean(param.damages) || Boolean(param.hasOwnProperty("specialEffects")) && Boolean(param.specialEffects))
            {
               if(param.hasOwnProperty("damages"))
               {
                  effectsTooltipBlockParams.showDamages = param.damages;
               }
               if(param.hasOwnProperty("specialEffects"))
               {
                  effectsTooltipBlockParams.showSpecialEffects = param.specialEffects;
               }
            }
            tooltip.addBlock(new HtmlEffectsTooltipBlock(effectsTooltipBlockParams).block);
         }
         if(setMode && _param.effects)
         {
            playerSetInfo = Api.player.getPlayerSet(this._itemData.objectGID);
            if(playerSetInfo)
            {
               effectsTooltipBlockParams = EffectsTooltipBlockParameters.create(playerSetInfo.setEffects,chunkType);
               effectsTooltipBlockParams.length = _param.length;
               effectsTooltipBlockParams.showTheoreticalEffects = showTheoretical;
               effectsTooltipBlockParams.setInfo = playerSetInfo.setName + " (" + playerSetInfo.setObjects.length + "/" + playerSetInfo.allItems.length + ")";
               tooltip.addBlock(new HtmlEffectsTooltipBlock(effectsTooltipBlockParams).block);
            }
         }
         if(this._itemData.typeId == DataEnum.ITEM_TYPE_IDOLS)
         {
            if(this._effects.length == 0)
            {
               tooltip.addBlock(new TextTooltipBlock(Api.ui.processText(Api.ui.getText("ui.common.effects"),"") + Api.ui.getText("ui.common.colon"),{"classCss":"subtitle"},chunkType).block);
            }
            idol = Api.data.getIdolByItemId(this._itemData.objectGID);
            tooltip.addBlock(new TextTooltipBlock("• " + idol.spellPair.description,{
               "classCss":"contentmargin",
               "width":_param.length
            },chunkType).block);
         }
         var cond:Object = this._itemData.conditions;
         if(Boolean(cond) && Boolean(cond.text) && _param.conditions)
         {
            tooltip.addBlock(new ConditionTooltipBlock(cond,null,false,chunkType).block);
         }
         var condT:Object = this._itemData.targetConditions;
         if(Boolean(condT) && Boolean(condT.text) && _param.targetConditions)
         {
            tooltip.addBlock(new ConditionTooltipBlock(condT,null,true,chunkType).block);
         }
         tooltip.addBlock(new ItemDetailsBlock(Boolean(data.hasOwnProperty("dropResult")) && Boolean(data.dropResult) ? data : this._itemData,_param,chunkType).block);
         if(_param.description)
         {
            tooltip.addBlock(new DescriptionTooltipBlock(this._itemData.description,"quote",chunkType).block);
         }
         if(!SpellTooltipMaker.SPELL_TAB_MODE && !_param.noFooter)
         {
            showTheoreticalEffectsTip = !this._itemData.enhanceable || showTheoretical || !theoreticalEffects || !theoreticalEffects.length || !effectsTooltipBlockParams ? "" : "[ui.tooltip.item.tip2]";
            if(sysApi.getOption("alwaysDisplayTheoreticalEffectsInTooltip","dofus"))
            {
               showTheoreticalEffectsTip = "";
            }
            leftText = "[ui.tooltip.item.tip]";
            if(Boolean(param) && Boolean(param.hasOwnProperty("noLeftFooter")) && Boolean(param.noLeftFooter))
            {
               leftText = "";
            }
            if(Boolean(leftText) || Boolean(showTheoreticalEffectsTip))
            {
               tooltip.addBlock(new TextWithTwoColumnsBlock({
                  "leftCss":"footerleft",
                  "leftText":leftText,
                  "rightCss":"footerright",
                  "rightText":showTheoreticalEffectsTip
               },chunkType).block);
            }
         }
         return tooltip;
      }
      
      private function compareEffectsOrder(effectA:EffectInstance, effectB:EffectInstance) : Number
      {
         var effectAIsNameEffect:Boolean = NAME_EFFECTS_IDS.indexOf(effectA.effectId) != -1;
         var effectBIsNameEffect:Boolean = NAME_EFFECTS_IDS.indexOf(effectB.effectId) != -1;
         if(!effectAIsNameEffect && effectBIsNameEffect)
         {
            return -1;
         }
         if(effectAIsNameEffect && !effectBIsNameEffect)
         {
            return 1;
         }
         return this._effects.indexOf(effectA) < this._effects.indexOf(effectB) ? -1 : 1;
      }
   }
}

class paramClass
{
   
   public var header:Boolean = true;
   
   public var effects:Boolean = true;
   
   public var description:Boolean = true;
   
   public var noBg:Boolean = false;
   
   public var CC_EC:Boolean = true;
   
   public var conditions:Boolean = true;
   
   public var targetConditions:Boolean = true;
   
   public var length:int = 409;
   
   public var averagePrice:Boolean = true;
   
   public var equipped:Boolean = false;
   
   public var shortcutKey:String;
   
   public var contextual:Boolean = false;
   
   public var noFooter:Boolean = false;
   
   public var addTheoreticalEffects:Boolean = false;
   
   public var noTheoreticalEffects:Boolean = false;
   
   public var showDropPercentage:Boolean = false;
   
   public function paramClass()
   {
      super();
   }
}
