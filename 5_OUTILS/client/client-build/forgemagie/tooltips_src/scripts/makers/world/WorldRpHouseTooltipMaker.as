package makers.world
{
   import blocks.HouseTooltipBlock;
   import d2hooks.*;
   
   public class WorldRpHouseTooltipMaker
   {
      
      public function WorldRpHouseTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/base.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new HouseTooltipBlock().block);
         return tooltip;
      }
   }
}

