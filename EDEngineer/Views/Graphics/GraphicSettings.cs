using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EDEngineer.Views.Graphics
{
    public class GraphicSettings : INotifyPropertyChanged
    {
        public GraphicSettings()
        {
            commanderRatio = Properties.Settings.Default.CommanderFontRatio;
            ingredientKindRatio = Properties.Settings.Default.IngredientKindFontRatio;
            ingredientRatio = Properties.Settings.Default.IngredientFontRatio;
            blueprintRatio = Properties.Settings.Default.BlueprintFontRatio;
            shoppingListRatio = Properties.Settings.Default.ShoppingListFontRatio;
            bottomRatio = Properties.Settings.Default.BottomFontRatio;
        }

        public void Sync()
        {
            Properties.Settings.Default.CommanderFontRatio = commanderRatio;
            Properties.Settings.Default.IngredientKindFontRatio = ingredientKindRatio;
            Properties.Settings.Default.IngredientFontRatio = ingredientRatio;
            Properties.Settings.Default.BlueprintFontRatio = blueprintRatio;
            Properties.Settings.Default.ShoppingListFontRatio = shoppingListRatio;
            Properties.Settings.Default.BottomFontRatio = bottomRatio;

            Properties.Settings.Default.Save();
        }

        private float commanderRatio;

        public float CommanderRatio
        {
            get { return commanderRatio; }
            set
            {
                if (Math.Abs(commanderRatio - value) < 0.01)
                {
                    return;
                }
                OnPropertyChanged();
                commanderRatio = value;
            }
        }

        private float ingredientKindRatio;

        public float IngredientKindRatio
        {
            get { return ingredientKindRatio; }
            set
            {
                if (Math.Abs(ingredientKindRatio - value) < 0.01)
                {
                    return;
                }
                OnPropertyChanged();
                ingredientKindRatio = value;
            }
        }

        private float ingredientRatio;

        public float IngredientRatio
        {
            get { return ingredientRatio; }
            set
            {
                if (Math.Abs(ingredientRatio - value) < 0.01)
                {
                    return;
                }
                OnPropertyChanged();
                ingredientRatio = value;
            }
        }

        private float blueprintRatio;

        public float BlueprintRatio
        {
            get { return blueprintRatio; }
            set
            {
                if (Math.Abs(blueprintRatio - value) < 0.01)
                {
                    return;
                }
                OnPropertyChanged();
                blueprintRatio = value;
            }
        }

        private float shoppingListRatio;

        public float ShoppingListRatio
        {
            get { return shoppingListRatio; }
            set
            {
                if (Math.Abs(shoppingListRatio - value) < 0.01)
                {
                    return;
                }
                OnPropertyChanged();
                shoppingListRatio = value;
            }
        }

        private float bottomRatio;

        public float BottomRatio
        {
            get { return bottomRatio; }
            set
            {
                if (Math.Abs(bottomRatio - value) < 0.01)
                {
                    return;
                }
                OnPropertyChanged();
                bottomRatio = value;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
