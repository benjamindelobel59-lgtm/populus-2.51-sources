package makers.world
{
   import blocks.mount.PaddockWithInstancesBlock;
   import d2hooks.*;
   
   public class WorldRpPaddockTooltipMaker
   {
      
      public function WorldRpPaddockTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/base.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new PaddockWithInstancesBlock().block);
         return tooltip;
      }
   }
}

