package makers
{
   import blocks.ConditionTooltipBlock;
   
   public class SellCriterionTooltipMaker
   {
      
      private static const chunkType:String = "htmlChunks";
      
      public function SellCriterionTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var bg:String = chunkType + "/base/baseWithBackground.txt";
         var tooltip:Object = Api.tooltip.createTooltip(bg,chunkType + "/base/container.txt",chunkType + "/base/separator.txt");
         tooltip.chunkType = chunkType;
         tooltip.addBlock(new ConditionTooltipBlock({"criteria":[data]},Api.ui.getText("ui.sell.condition"),false,chunkType).block);
         return tooltip;
      }
   }
}

