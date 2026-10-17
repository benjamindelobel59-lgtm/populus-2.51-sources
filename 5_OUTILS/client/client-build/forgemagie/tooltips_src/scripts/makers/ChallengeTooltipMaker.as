package makers
{
   import blocks.ChallengeTooltipBlock;
   import blocks.DescriptionTooltipBlock;
   import blocks.TextTooltipBlock;
   import d2enums.ProtocolConstantsEnum;
   import d2hooks.*;
   
   public class ChallengeTooltipMaker
   {
      
      private var _param:paramClass;
      
      public function ChallengeTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var target:String = null;
         var description:String = null;
         var uiApi:Object = Api.ui;
         this._param = new paramClass();
         if(param)
         {
            if(param.hasOwnProperty("name"))
            {
               this._param.name = param.name;
            }
            if(param.hasOwnProperty("description"))
            {
               this._param.description = param.description;
            }
            if(param.hasOwnProperty("effects"))
            {
               this._param.effects = param.effects;
            }
         }
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/baseWithBackground.txt","chunks/base/container.txt","chunks/base/separator.txt");
         if(this._param.name)
         {
            tooltip.addBlock(new TextTooltipBlock(data.name,{
               "content":data.name,
               "css":"[local.css]tooltip_title.css",
               "cssClass":"spell"
            }).block);
         }
         if(this._param.description)
         {
            target = data.targetName + " (";
            if(data.targetId > 0 && data.targetLevel > ProtocolConstantsEnum.MAX_LEVEL)
            {
               target += uiApi.getText("ui.common.short.prestige") + (data.targetLevel - ProtocolConstantsEnum.MAX_LEVEL) + ")";
            }
            else
            {
               target += uiApi.getText("ui.common.short.level") + data.targetLevel + ")";
            }
            description = data.description;
            description = description.replace("%1",target);
            tooltip.addBlock(new DescriptionTooltipBlock(description).block);
         }
         if(this._param.effects)
         {
            tooltip.addBlock(new ChallengeTooltipBlock(data).block);
         }
         return tooltip;
      }
   }
}

class paramClass
{
   
   public var name:Boolean = true;
   
   public var description:Boolean = true;
   
   public var effects:Boolean = true;
   
   public function paramClass()
   {
      super();
   }
}
