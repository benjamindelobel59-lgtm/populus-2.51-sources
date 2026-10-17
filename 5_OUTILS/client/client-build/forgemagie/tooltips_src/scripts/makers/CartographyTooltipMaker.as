package makers
{
   import blocks.CartographyTooltipBlock;
   
   public class CartographyTooltipMaker
   {
      
      public function CartographyTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/base.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new CartographyTooltipBlock(data).block);
         return tooltip;
      }
   }
}

