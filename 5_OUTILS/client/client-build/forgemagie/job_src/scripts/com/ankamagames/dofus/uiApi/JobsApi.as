package com.ankamagames.dofus.uiApi
{
   import com.ankamagames.dofus.datacenter.jobs.Recipe;
   import com.ankamagames.dofus.datacenter.jobs.Skill;
   import com.ankamagames.dofus.internalDatacenter.items.ItemWrapper;
   import com.ankamagames.dofus.internalDatacenter.jobs.KnownJobWrapper;
   
   public class JobsApi
   {
      
      public function JobsApi()
      {
         super();
      }
      
      public function destroy() : void
      {
      }
      
      public function getCurrentCraftSkillId() : int
      {
         return 0;
      }
      
      public function getJobSkills(jobId:int) : Array
      {
         return null;
      }
      
      public function getJobSkillType(jobId:int, skill:Skill) : String
      {
         return null;
      }
      
      public function getJobCollectSkillInfos(jobId:int, skill:Skill) : Object
      {
         return null;
      }
      
      public function getMaxSlotsByJobId(jobId:int) : int
      {
         return 0;
      }
      
      public function getJobExperience(jobId:int) : Object
      {
         return null;
      }
      
      public function getCraftXp(item:ItemWrapper, jobLevel:uint) : int
      {
         return 0;
      }
      
      public function getSkillFromId(skillId:int) : Skill
      {
         return null;
      }
      
      public function getRecipe(objectId:uint) : Recipe
      {
         return null;
      }
      
      public function craftHasCriteria(objectId:uint) : Boolean
      {
         return false;
      }
      
      public function checkCraftVisible(objectId:uint) : Boolean
      {
         return false;
      }
      
      public function checkCraftFeasible(objectId:uint) : Boolean
      {
         return false;
      }
      
      public function getRecipesList(objectId:uint) : Array
      {
         return null;
      }
      
      public function getJobName(pJobId:uint) : String
      {
         return null;
      }
      
      public function getJob(pJobId:uint) : Object
      {
         return null;
      }
      
      public function getJobCrafterDirectorySettingsById(jobId:uint) : Object
      {
         return null;
      }
      
      public function getUsableSkillsInMap(playerId:Number) : Array
      {
         return null;
      }
      
      public function getKnownJobs() : Array
      {
         return null;
      }
      
      public function getKnownJob(jobId:uint) : KnownJobWrapper
      {
         return null;
      }
      
      public function getRecipesByJob(details:Object, skillId:int = 0, jobId:int = 0, fromBank:Boolean = false, missingIngredientsTolerance:int = 8) : Object
      {
         return null;
      }
      
      public function getJobFilteredRecipes(recipes:Object, resultTypes:Array, minLevel:int = 1, maxLevel:int = 1, search:String = null, typeId:int = 0) : Array
      {
         return null;
      }
      
      public function getInventoryData(fromBank:Boolean = false) : Array
      {
         return null;
      }
      
      public function sortRecipesByCriteria(recipes:Object, sortCriteria:String, sortDescending:Boolean) : Object
      {
         return null;
      }
   }
}

