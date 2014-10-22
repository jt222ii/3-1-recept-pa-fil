using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        public void Show(IRecipe recipe)
        {
            //Header, ShowHeader och ContinueOnKeyPressed
            //Header får namnet av det valda receptet. ShowHeaderPanel(); använder sig av Header och skriver ut en panel med korrekt namn.
            Header = recipe.Name;
            ShowHeaderPanel(); //kallar på ShowHeaderPanel
            //skriva ut recept - foreach som han visade på föreläsningen
            //för varje ingrediens i ingredienser
            Console.WriteLine("\nIngredienser\n------------");
            foreach(Ingredient ingredient in recipe.Ingredients)
            {
                Console.WriteLine(ingredient);
            }
            //skriva ut instruktioner - foreach här också
            //för varje instruktion i instruktioner
            Console.WriteLine("\nGör Så Här\n----------");
            foreach(string instruction in recipe.Instructions)
            {
                Console.WriteLine(instruction);
            }


        }
        public void Show(IEnumerable<IRecipe> recipe)
        {

        }
    }
}
