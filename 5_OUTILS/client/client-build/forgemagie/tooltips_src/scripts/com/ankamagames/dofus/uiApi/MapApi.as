package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.world.MapPosition;
   import com.ankamagames.dofus.datacenter.world.SubArea;
   import com.ankamagames.dofus.datacenter.world.WorldMap;
   import com.ankamagames.jerakine.types.positions.WorldPoint;
   import flash.geom.Point;
   
   public class MapApi
   {
      
      public function MapApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getCurrentSubArea() : SubArea
      {
         return null;
      }
      
      public function getCurrentWorldMap() : WorldMap
      {
         return null;
      }
      
      public function getAllSuperArea() : Array
      {
         return null;
      }
      
      public function getAllArea() : Array
      {
         return null;
      }
      
      public function getAllSubArea() : Array
      {
         return null;
      }
      
      public function getSubArea(subAreaId:uint) : SubArea
      {
         return null;
      }
      
      public function getSubAreaMapIds(subAreaId:uint) : Object
      {
         return null;
      }
      
      public function getSubAreaCenter(subAreaId:uint) : Point
      {
         return null;
      }
      
      public function getWorldPoint(mapId:Number) : WorldPoint
      {
         return null;
      }
      
      public function getMapCoords(mapId:Number) : Point
      {
         return null;
      }
      
      public function getMapTotalRewardRate() : int
      {
         return 0;
      }
      
      public function getSubAreaShape(subAreaId:uint) : Object
      {
         return null;
      }
      
      public function getMapShape(pX:int, pY:int) : Object
      {
         return null;
      }
      
      public function getHints() : Array
      {
         return null;
      }
      
      public function getShortcuts(worldMap:int) : Array
      {
         return null;
      }
      
      public function subAreaByMapId(mapId:Number) : SubArea
      {
         return null;
      }
      
      public function getMapIdByCoord(x:int, y:int) : Object
      {
         return null;
      }
      
      public function getMapPositionById(mapId:Number) : MapPosition
      {
         return null;
      }
      
      public function intersects(rect1:Object, rect2:Object) : Boolean
      {
         return false;
      }
      
      public function autoTravel(x:int, y:int) : void
      {
      }
      
      public function movePlayer(x:int, y:int, world:int = -1) : void
      {
      }
      
      public function movePlayerOnMapId(mapId:Number) : void
      {
      }
      
      public function getMapReference(refId:uint) : Object
      {
         return null;
      }
      
      public function getPhoenixsMaps() : Array
      {
         return null;
      }
      
      public function isInIncarnam() : Boolean
      {
         return false;
      }
      
      public function getSearchAllResults(pSearch:String) : Object
      {
         return null;
      }
      
      public function isDungeonSubArea(pSubAreaId:int) : int
      {
         return 0;
      }
   }
}

