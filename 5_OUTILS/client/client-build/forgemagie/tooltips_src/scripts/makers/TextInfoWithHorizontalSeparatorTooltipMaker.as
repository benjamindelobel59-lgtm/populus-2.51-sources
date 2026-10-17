package makers
{
   import blocks.TextInfoWithHorizontalSeparatorTooltipBlock;
   
   public class TextInfoWithHorizontalSeparatorTooltipMaker
   {
      
      public function TextInfoWithHorizontalSeparatorTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/baseWithBackground.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new TextInfoWithHorizontalSeparatorTooltipBlock(data).block);
         return tooltip;
      }
   }
}

