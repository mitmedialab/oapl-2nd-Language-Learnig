using System;
using System.Collections.Generic;

namespace opal
{
	public class AnimalHandler
	{
		public Dictionary<string,int> animalsCount;
		public List<string> NewAnimalList;

		/// <summary>
		/// AnimalsTracker constructor
		/// </summary>
		public AnimalHandler ()
		{
			NewAnimalList = new List<string> (new string[]{Constants.ANIMAL_CHICKEN,
				Constants.ANIMAL_DINOSAUR
				 });
			animalsCount = new Dictionary<string, int>();

		}

		/// <summary>
		/// Return an animal that has never appeared in the game so far
		/// </summary>
		public string GetNewAnimal()
		{
			Random rnd = new Random ();
			int animalIndex = rnd.Next (0,NewAnimalList.Count);
			string newAnimal = NewAnimalList [animalIndex];
			//NewAnimalList.RemoveAt (animalIndex);
			return newAnimal;
		}


	}
}

