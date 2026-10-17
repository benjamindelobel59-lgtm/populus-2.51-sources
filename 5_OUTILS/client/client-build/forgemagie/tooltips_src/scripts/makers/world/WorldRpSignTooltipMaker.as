package makers.world
{
   import blocks.SignTooltipBlock;
   
   public class WorldRpSignTooltipMaker
   {
      
      public function WorldRpSignTooltipMaker()
      {
         super();
      }
      
      public function createTooltip(data:*, param:Object) : Object
      {
         var tooltip:Object = Api.tooltip.createTooltip("chunks/base/baseWithBackground.txt","chunks/base/container.txt","chunks/base/empty.txt");
         tooltip.addBlock(new SignTooltipBlock(data).block);
         return tooltip;
      }
   }
}

