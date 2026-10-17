package makers
{
   import blocks.BreachMonstersBlock;
   
   public class BreachMonstersGroupTooltipMaker
   {
      
      public function BreachMonstersGroupTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/base.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new BreachMonstersBlock().block);
         return tooltip;
      }
   }
}

