using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        } 


        public void Load()
        {
           // List<string> recipes = new List<string>();
            List<IRecipe> recipes = new List<IRecipe>();
            Recipe fullRecipe = null;
            RecipeReadStatus recipeReadStatus = new RecipeReadStatus(); //enum
            //ska "using" användas som han visade på föreläsningen?
            //2. Öppna textfilen för läsning.
            using (StreamReader reader = new StreamReader(@"Recipes.txt"))
            {
                //@"C:\Users\Jonas\Desktop\Recept på fil\3-1-recept-pa-fil\FiledRecipes\FiledRecipes\App_Data\Recipes.txt")
                string line;
                //3. Läs rad från textfilen tills det är slut på filen.
                while ((line = reader.ReadLine()) != null)
                {
                    switch(line)
                    { 
                        case SectionRecipe:// SectionRecipe och de andra är konstanter som skapats ovan
                            recipeReadStatus = RecipeReadStatus.New;
                            continue;
                        case SectionIngredients:
                            recipeReadStatus = RecipeReadStatus.Ingredient;
                            continue;
                        case SectionInstructions:
                            recipeReadStatus = RecipeReadStatus.Instruction;
                            continue;
                    }
                            //ska det vara else?
                    switch(recipeReadStatus)
                    {
                        case RecipeReadStatus.New:
                            //skapa ett nytt receptobjekt med receptets namn
                            fullRecipe = new Recipe(line);
                            recipes.Add(fullRecipe);
                            break;
                        case RecipeReadStatus.Ingredient:
                            //1. Dela upp raden i delar genom att använda metoden Split() i klassen 
                            //String. De olika delarna separeras åt med semikolon varför det 
                            //alltid ska bli tre delar.
                            string[] ingredients = line.Split(new string[] { ";" }, StringSplitOptions.None);
                            //2. Om antalet delar inte är tre…
                            //a. …är något fel varför ett undantag av typen 
                            //FileFormatException ska kastas.
                            if (ingredients.Length % 3 != 0)
                            {
                                throw new FileFormatException();
                            }
                            //3. Skapa ett ingrediensobjekt och initiera det med de tre delarna för 
                            //mängd, mått och namn.
                            Ingredient ingredient = new Ingredient();
                            ingredient.Amount = ingredients[0]; // 0 för att "4,5;dl;filmjölk" blir [0];[1];[2]
                            ingredient.Measure = ingredients[1];
                            ingredient.Name = ingredients[2];
                            //4. Lägg till ingrediensen till receptets lista med ingredienser
                            //foreach (string ingredientToList in ingredients)
                            //{
                            //    recipes.Add(ingredientToList);
                            //}
                            fullRecipe.Add(ingredient);
                            break;
                        case RecipeReadStatus.Instruction:
                            //Lägg till raden till receptets lista med instruktioner.
                            fullRecipe.Add(line);
                            break;
                        case RecipeReadStatus.Indefinite:                       
                            //…är något fel varför ett undantag av typen FileFormatException ska kastas.
                            throw new FileFormatException();                       
                    } 
                
                }
                //4. Sortera listan med recept med avseende på receptens namn.
                //5. Tilldela avsett fält i klassen, _recipes, en referens till listan.
                _recipes =  recipes.OrderBy(recipe => recipe.Name ).ToList();
                //recipes.TrimExcess(); // tar bort de tomma "lådorna" i den dynamiska arrayen recipes.
                
                //6. Tilldela avsedd egenskap i klassen, IsModified, ett värde som indikerar att listan med recept 
                //är oförändrad.
                IsModified= false;
                //7. Utlös händelse om att recept har lästs in genom att anropa metoden OnRecipesChanged och 
                //skicka med parametern EventArgs.Empty.
                OnRecipesChanged(EventArgs.Empty);

            }
        }
        public void Save()
        { 
            //Recept ska sparas permanent i textfilen recipes.txt. Väljer användaren menyalternativet 
            //’2. Spara’ ska applikationen öppna textfilen och skriva recepten rad för rad till textfilen. Finns redan 
            //textfilen ska den skrivas över.
            //streamwriter? antagligen
            using (StreamWriter writer = new StreamWriter(@"Recipes.txt"))   //http://msdn.microsoft.com/en-us/library/8bh11f1k.aspx
            {
                //för varje recept skriv receptet
                foreach (Recipe recipe in _recipes)
                {
                    //skriv "[Recept]" och receptets namn - [Recept] -> SectionRecipe
                    writer.WriteLine(SectionRecipe);
                    writer.WriteLine(recipe.Name);
                    //skriv "[Ingredienser]" och ingredienserna - [Ingredienser] -> SectionIngredients | Ingredienserna är det man gav dem ovan. ingredient.Amount, ingredient.Measure och ingredient.Name
                    writer.WriteLine(SectionIngredients);

                    //skriv "[Instruktioner]" och instruktionerna - [Instruktioner] -> SectionInstuctions | recipe.Instructions
                    //för varje 
                }
            }

        }
    }
}
