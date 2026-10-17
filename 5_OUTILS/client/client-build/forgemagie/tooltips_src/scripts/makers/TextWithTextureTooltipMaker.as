package makers
{
   import blocks.TextWithTextureTooltipBlock;
   
   public class TextWithTextureTooltipMaker
   {
      
      public function TextWithTextureTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/baseWithBackground.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new TextWithTextureTooltipBlock(data).block);
         return tooltip;
      }
   }
}

