package makers
{
   import blockParams.EffectsTooltipBlockParameters;
   import blocks.EffectsTooltipBlock;
   import blocks.SeparatorTooltipBlock;
   import d2hooks.*;
   
   public class EffectsListDurationTooltipMaker
   {
      
      public function EffectsListDurationTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var bg:String = null;
         var effectList:Array = null;
         var effectsTooltipBlockParams:EffectsTooltipBlockParameters = null;
         var e:Object = null;
         var dataApi:Object = Api.data;
         if(Boolean(param) && Boolean(param.noBg))
         {
            bg = "chunks/base/base.txt";
         }
         else
         {
            bg = "chunks/base/baseWithBackground.txt";
         }
         var tooltip:Object = Api.tooltip.createTooltip(bg,"chunks/base/container.txt","chunks/base/separator.txt");
         var lastTurn:int = -1;
         for each(e in data)
         {
            if(e.duration != lastTurn)
            {
               if(effectList)
               {
                  effectsTooltipBlockParams = EffectsTooltipBlockParameters.create(effectList);
                  effectsTooltipBlockParams.length = 422;
                  effectsTooltipBlockParams.showLabel = false;
                  tooltip.addBlock(new EffectsTooltipBlock(effectsTooltipBlockParams).block);
                  tooltip.addBlock(new SeparatorTooltipBlock().block);
               }
               lastTurn = int(e.duration);
               effectList = new Array();
            }
            effectList.push(e);
         }
         effectsTooltipBlockParams = EffectsTooltipBlockParameters.create(effectList);
         effectsTooltipBlockParams.length = 422;
         effectsTooltipBlockParams.showLabel = false;
         tooltip.addBlock(new EffectsTooltipBlock(effectsTooltipBlockParams).block);
         tooltip.addBlock(new SeparatorTooltipBlock().block);
         return tooltip;
      }
   }
}

