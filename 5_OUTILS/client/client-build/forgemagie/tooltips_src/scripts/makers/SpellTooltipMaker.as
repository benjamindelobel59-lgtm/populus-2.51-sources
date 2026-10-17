package makers
{
   import blockParams.EffectsTooltipBlockParameters;
   import blocks.DescriptionTooltipBlock;
   import blocks.HtmlEffectsTooltipBlock;
   import blocks.SpellHeaderBlock;
   import blocks.SpellPairHeaderBlock;
   import blocks.TextTooltipBlock;
   import blocks.TextWithTwoColumnsBlock;
   import com.ankamagames.dofus.datacenter.spells.SpellPair;
   import com.ankamagames.dofus.internalDatacenter.spells.SpellWrapper;
   import d2hooks.*;
   
   public class SpellTooltipMaker
   {
      
      public static var SPELL_TAB_MODE:Boolean;
      
      private static const chunkType:String = "htmlChunks";
      
      private var _param:paramClass;
      
      public function SpellTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var bg:String = null;
         var weapon:Object = null;
         var effectsTooltipBlockParams:EffectsTooltipBlockParameters = null;
         var dataApi:Object = Api.data;
         this._param = new paramClass();
         SPELL_TAB_MODE = Boolean(param) && param.hasOwnProperty("spellTab") && Boolean(param.spellTab);
         var advanced:Boolean = Boolean(param) && (param.hasOwnProperty("advanced") && param.advanced || param.hasOwnProperty("alwaysAdvanced") && param.alwaysAdvanced) || Boolean(Api.system.getOption("useAdvancedSpellTooltips","dofus"));
         this._param.effects = advanced ? true : false;
         this._param.CC_EC = advanced ? true : false;
         if(param)
         {
            param.CC_EC = this._param.CC_EC;
            if(param.hasOwnProperty("noBg"))
            {
               this._param.noBg = param.noBg;
            }
            if(param.hasOwnProperty("description"))
            {
               this._param.description = param.description;
            }
            if(param.hasOwnProperty("smallSpell"))
            {
               this._param.smallSpell = param.smallSpell;
            }
            if(param.hasOwnProperty("name"))
            {
               this._param.name = param.name;
            }
            if(param.hasOwnProperty("header"))
            {
               this._param.header = param.header;
            }
            if(param.hasOwnProperty("contextual"))
            {
               this._param.contextual = param.contextual;
            }
            if(param.hasOwnProperty("shortcutKey"))
            {
               this._param.shortcutKey = param.shortcutKey;
            }
            if(param.hasOwnProperty("footer"))
            {
               this._param.footer = param.footer;
            }
         }
         if(!this._param.CC_EC && !this._param.description && !this._param.effects && !this._param.name)
         {
            return "";
         }
         if(this._param.noBg)
         {
            bg = chunkType + "/spell/base";
         }
         else
         {
            bg = chunkType + "/spell/baseWithBackground";
         }
         if(this._param.name)
         {
            bg += ".txt";
         }
         else
         {
            bg += "NoIcon.txt";
         }
         var tooltip:Object = Api.tooltip.createTooltip(bg,chunkType + "/base/container.txt",chunkType + "/base/separator.txt");
         tooltip.chunkType = chunkType;
         var spellItem:Object = data;
         if(spellItem is SpellWrapper && Boolean(spellItem.isSpellWeapon))
         {
            weapon = Api.player.getWeapon();
            if(weapon)
            {
               return new ItemTooltipMaker().createTooltip(weapon,{
                  "noBg":this._param.noBg,
                  "header":this._param.name,
                  "effects":this._param.effects,
                  "contextual":this._param.contextual,
                  "conditions":true,
                  "description":this._param.description,
                  "CC_EC":this._param.CC_EC,
                  "contextual":true,
                  "shortcutKey":this._param.shortcutKey
               });
            }
         }
         if(this._param.header)
         {
            if(spellItem is SpellPair)
            {
               tooltip.addBlock(new SpellPairHeaderBlock(spellItem as SpellPair).block);
            }
            else
            {
               tooltip.addBlock(new SpellHeaderBlock(spellItem,param,chunkType).block);
            }
         }
         if(this._param.description && Boolean(spellItem.description))
         {
            tooltip.addBlock(new DescriptionTooltipBlock(spellItem.description,"description",chunkType).block);
         }
         if(spellItem is SpellPair)
         {
            return tooltip;
         }
         if(this._param.effects)
         {
            if(!spellItem.hideEffects && Boolean(spellItem.effects.length))
            {
               effectsTooltipBlockParams = EffectsTooltipBlockParameters.create(spellItem.effects);
               effectsTooltipBlockParams.chunkType = chunkType;
               effectsTooltipBlockParams.splitDamageAndEffects = false;
               tooltip.addBlock(new HtmlEffectsTooltipBlock(effectsTooltipBlockParams).block);
            }
            if(!spellItem.hideEffects && Boolean(spellItem.criticalEffect.length))
            {
               effectsTooltipBlockParams = EffectsTooltipBlockParameters.create(spellItem.criticalEffect);
               effectsTooltipBlockParams.isCriticalEffects = true;
               effectsTooltipBlockParams.splitDamageAndEffects = false;
               effectsTooltipBlockParams.chunkType = chunkType;
               tooltip.addBlock(new HtmlEffectsTooltipBlock(effectsTooltipBlockParams).block);
            }
         }
         if(!SPELL_TAB_MODE && this._param.footer)
         {
            if(!advanced)
            {
               tooltip.addBlock(new TextWithTwoColumnsBlock({
                  "leftCss":"footerleft",
                  "leftText":(Boolean(param) && param.footerText != null ? param.footerText : Api.ui.getText("ui.tooltip.spell.tip")),
                  "rightCss":"footerright",
                  "rightText":"[Ctrl] " + Api.ui.getText("ui.tooltip.advanced")
               },chunkType).block);
            }
            else if(Boolean(param) && Boolean(param.hasOwnProperty("alwaysAdvanced")) && Boolean(param.alwaysAdvanced) && (!param.hasOwnProperty("footerText") || param.footerText == null))
            {
               tooltip.addBlock(new TextTooltipBlock(Api.ui.getText("ui.tooltip.spell.tip"),{"classCss":"footerleft"},chunkType).block);
            }
            else
            {
               tooltip.addBlock(new TextWithTwoColumnsBlock({
                  "leftCss":"footerleft",
                  "leftText":(Boolean(param) && param.footerText != null ? param.footerText : Api.ui.getText("ui.tooltip.spell.tip")),
                  "rightCss":"footerright",
                  "rightText":"[Ctrl] " + Api.ui.getText("ui.tooltip.simplified")
               },chunkType).block);
            }
         }
         return tooltip;
      }
   }
}

class paramClass
{
   
   public var contextual:Boolean = false;
   
   public var name:Boolean = true;
   
   public var header:Boolean = true;
   
   public var footer:Boolean = true;
   
   public var description:Boolean = true;
   
   public var effects:Boolean = true;
   
   public var noBg:Boolean = false;
   
   public var smallSpell:Boolean = false;
   
   public var CC_EC:Boolean = true;
   
   public var shortcutKey:String = "";
   
   public function paramClass()
   {
      super();
   }
}
