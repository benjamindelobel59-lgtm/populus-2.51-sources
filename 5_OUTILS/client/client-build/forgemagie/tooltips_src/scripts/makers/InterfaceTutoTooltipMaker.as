package makers
{
   import blocks.InterfaceTutoTooltipBlock;
   
   public class InterfaceTutoTooltipMaker
   {
      
      public function InterfaceTutoTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/baseWithButton.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new InterfaceTutoTooltipBlock(data).block);
         return tooltip;
      }
   }
}

