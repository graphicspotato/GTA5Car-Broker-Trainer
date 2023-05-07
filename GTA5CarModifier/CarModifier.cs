using System;
using System.Collections.Generic;
using System.Linq;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using GTA.Native;
using LemonUI;
using LemonUI.Menus;
using GTA.UI;
using System.Globalization;
using GTA5CarModifier.Config;


namespace GTA5CarBroker
{
    
    public static class OutputMessages
    {
        public static void ShowNotInVehicleMessage()
        {
            Notification.Show("You are not in a vehicle.");
        }

        public static void ShowInvalidInputMessage() 
        {
            Notification.Show("Invalid or null value entered.");
        }
    }
        
    public class CarBroker: Script
    {

        private readonly FlyKeyConfig _flyKeyConfig;

        #region MENU
        private ObjectPool pool = new ObjectPool();
        private NativeMenu menu = new NativeMenu("Car Broker");
        
        private NativeItem enginePowerItem = new NativeItem("Engine Power");
        private NativeItem torqueItem = new NativeItem("Torque");
        private NativeItem vehicleRepairItem = new NativeItem("Repair Damage");
        private NativeCheckboxItem vehicleGodModeItem = new NativeCheckboxItem("Vehicle God Mode");
        private NativeCheckboxItem playerGodModeItem = new NativeCheckboxItem("Player God Mode");
        private NativeItem vehicleListMenuItem = new NativeItem("Vehicle List");
        private NativeItem vehicleLightsMultiItem = new NativeItem("Vehicle Lights Multiplier (Epilepsy Warning!)", "Mind Your Input");
        private NativeItem vehicleMassItem = new NativeItem("Vehicle Mass");
        private NativeItem forceMenuItem = new NativeItem("Force Menu");
        private NativeItem forceActivateItem = new NativeItem("Force Me");
        private NativeCheckboxItem vehicleStopCheckboxItem = new NativeCheckboxItem("Stop.");

        private NativeCheckboxItem aggressiveDriversCheckBoxItem = new NativeCheckboxItem("Aggressive Drivers");
        private NativeItem tractionInputItem = new NativeItem("Traction Modifier", "2 Is Default");
        

        private NativeMenu forceMenu = new NativeMenu("Force Menu");
        private NativeItem forceInputItem = new NativeItem("Input a Force Value");
        private NativeCheckboxItem carCanForceDrivable = new NativeCheckboxItem("Vehicle Can Drivable By Force: ");
        #endregion

        #region Singleton
        public class VehicleManager
        {
            private static readonly Lazy<VehicleManager> lazy =
                new Lazy<VehicleManager>(() => new VehicleManager());

            public static VehicleManager Instance { get { return lazy.Value; } }

            private VehicleManager() { }
            public Vehicle CurrentVehicle
            {
                get
                {
                    return Game.Player.Character.CurrentVehicle;
                }
            }
        }
        #endregion

        #region Variables
        float forceVal;

        bool aggressiveDriversMode = false;

        int forceMenuItemCounter = 0;

        bool gotForce = false;

        private bool isMenuOpen = false;

        private bool carCanDrivableByForce = false;
        #endregion

        #region Constructor
        public CarBroker() 
        {
            _flyKeyConfig = new FlyKeyConfig(Settings);
            
            
            this.Tick += OnTick;
            this.KeyDown += OnKeyDown;
            
            menu.Add(vehicleListMenuItem);
            menu.Add(vehicleMassItem);
            menu.Add(enginePowerItem);
            menu.Add(torqueItem);
            menu.Add(tractionInputItem);
            menu.Add(vehicleRepairItem);
            menu.Add(vehicleLightsMultiItem);
            menu.Add(forceMenuItem);
            menu.Add(vehicleGodModeItem);
            menu.Add(playerGodModeItem);
            menu.Add(vehicleStopCheckboxItem);
            menu.Add(aggressiveDriversCheckBoxItem);

            
            forceMenu.Add(forceInputItem);
            forceMenu.Add(forceActivateItem);
            forceMenu.Add(carCanForceDrivable);

            forceInputItem.Activated += OnForceMenuForceInputActivated;
            forceActivateItem.Activated += OnForceActivateCheck;
            carCanForceDrivable.Activated += OnCarCanForceDrivable;

            forceMenuItem.Activated += OnForceMenuItemActivated;
            forceActivateItem.Activated += OnForceActivateCheck;

            vehicleListMenuItem.Activated += OnVehicleListActivated;
            enginePowerItem.Activated += OnEnginePowerActivated;
            torqueItem.Activated += OnTorqueActivated;
            vehicleRepairItem.Activated += OnRepairActivated;
            vehicleLightsMultiItem.Activated += OnLightsMultiplierActivated;
            vehicleMassItem.Activated += OnVehicleMassActivated;
            vehicleGodModeItem.Activated += OnGodModeChecked;
            vehicleStopCheckboxItem.Activated += OnVehicleStopActivated;
            aggressiveDriversCheckBoxItem.Activated += OnAggressiveDriversActivated;
            tractionInputItem.Activated += OnTractionInputActivated;
            playerGodModeItem.Activated += OnPlayerGodModeActivated;




            pool.Add(menu);
            pool.Add(forceMenu);

        }
        #endregion

        #region OnTick
        private NativeItem selectedMenuItem = null;
        private void OnTick(object sender, EventArgs e)
        {
            pool.Process();

            menu.UseMouse = false;

            int selectedIndex = menu.SelectedIndex;

            NativeItem newSelectedMenuItem = menu.Items[selectedIndex];

            if (selectedMenuItem != newSelectedMenuItem)
            {
                
                menu.SoundUpDown?.PlayFrontend();

                // Update the selected menu item variable
                selectedMenuItem = newSelectedMenuItem;
            }

            //to check that if user is in the car to activate godmode item to make it checkable.
            bool isInVehicle = Game.Player.Character.IsInVehicle();
            
            if (!isInVehicle)
            {
                vehicleGodModeItem.Checked = false;
                
                vehicleGodModeItem.Enabled = false;
            }
            else
            {
                vehicleGodModeItem.Enabled = true;
            }
            //                                                   

            foreach (Vehicle vehicle in World.GetNearbyVehicles(Game.Player.Character.Position, 100f))
            {
                if (vehicle.Driver != null && vehicle.Exists())
                {
                    if (aggressiveDriversMode)
                    {
                        vehicle.Driver.DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                    }
                    else
                    {
                        vehicle.Driver.DrivingStyle = DrivingStyle.Normal;
                    }
                }
            }

            if(carCanDrivableByForce == true && VehicleManager.Instance.CurrentVehicle != null)
            {
                if (Game.IsKeyPressed(_flyKeyConfig.UpwardDirection))
                {
                    VehicleManager.Instance.CurrentVehicle.ApplyForce(VehicleManager.Instance.CurrentVehicle.UpVector*forceVal);
                    //Notification.Show("fly");
                }

                if (Game.IsKeyPressed(Keys.Y))
                {
                    Vehicle currentVehicle = VehicleManager.Instance.CurrentVehicle;
                    Vector3 backwardVector = -currentVehicle.UpVector;
                    currentVehicle.ApplyForce(backwardVector * forceVal);
                }

                if (Game.IsKeyPressed(Keys.L))
                {
                    VehicleManager.Instance.CurrentVehicle.ApplyForce(VehicleManager.Instance.CurrentVehicle.RightVector * forceVal);
                }
                if (Game.IsKeyPressed(Keys.J))
                {

                    Vehicle currentVehicle = VehicleManager.Instance.CurrentVehicle;
                    Vector3 leftVector = Vector3.Cross(currentVehicle.UpVector, currentVehicle.ForwardVector);
                    currentVehicle.ApplyForce(leftVector * forceVal);

                }
                if (Game.IsKeyPressed(Keys.I))
                {
                    VehicleManager.Instance.CurrentVehicle.ApplyForce(VehicleManager.Instance.CurrentVehicle.ForwardVector * forceVal);
                }

                if (Game.IsKeyPressed(Keys.K))
                {
                    Vehicle currentVehicle = VehicleManager.Instance.CurrentVehicle;
                    Vector3 backwardVector = -currentVehicle.ForwardVector;
                    currentVehicle.ApplyForce(backwardVector * forceVal);
                }


            }


        }
        #endregion

        #region OnKeyDown
        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.M)
            {
                
                menu.Visible = true;

            }
        }
        #endregion

        #region OnEnginePowerActivated
        private void OnEnginePowerActivated(object sender, EventArgs e)
        
        {
            menu.SoundOpened?.PlayFrontend();
            
            int newEnginePower;

            if (VehicleManager.Instance.CurrentVehicle != null)
            {
                if (int.TryParse(Game.GetUserInput(), out newEnginePower))
                {
                    VehicleManager.Instance.CurrentVehicle.EnginePowerMultiplier = newEnginePower;
                    menu.Visible = false;
                }

                else
                {
                    OutputMessages.ShowInvalidInputMessage();
                }
            }

            else
            {
                OutputMessages.ShowNotInVehicleMessage();
            }
        }
        #endregion

        #region OnTorqueActivated
        private void OnTorqueActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();
            
            int newTorqueValue;
            
            if (VehicleManager.Instance.CurrentVehicle != null)
            {
                if (int.TryParse(Game.GetUserInput(), out newTorqueValue))
                {
                    VehicleManager.Instance.CurrentVehicle.EngineTorqueMultiplier = newTorqueValue;
                    menu.Visible = false;
                }

                else
                {
                    OutputMessages.ShowInvalidInputMessage();
                }
            }

            else
            {
                OutputMessages.ShowNotInVehicleMessage();
            }

        }
        #endregion

        #region OnRepairActivated
        private void OnRepairActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();
           
            if (VehicleManager.Instance.CurrentVehicle != null)
            {
                VehicleManager.Instance.CurrentVehicle.Repair();
            }

            else
            {
                OutputMessages.ShowNotInVehicleMessage();
            }
        }
        #endregion

        #region OnVehicleListActivated
        private void OnVehicleListActivated(object sender, EventArgs e)
        {

            menu.SoundOpened?.PlayFrontend();
            
            isMenuOpen = true;
           
            List<Vehicle> allVehicles = World.GetAllVehicles().ToList();

            NativeMenu vehicleListMenu = new NativeMenu("Vehicle List");

            vehicleListMenu.UseMouse = false;

            var vehicleHashes = Enum.GetValues(typeof(VehicleHash))
    .Cast<VehicleHash>()
    .Where(h => Function.Call<bool>(Hash.IS_MODEL_A_VEHICLE, (int)h))
    .OrderBy(h => h.ToString()) // sort alphabetically by name
    .ToList();

            Vehicle spawnedVehicle = null;
            // Loop through the vehicle hashes and add each one to the menu
            foreach (var vehicleHash in vehicleHashes)
            {
                NativeItem vehicleItem = new NativeItem(vehicleHash.ToString());
                vehicleItem.Activated += (s, ev) =>
                {
                    if (spawnedVehicle != null)
                    {
                        spawnedVehicle.Delete();
                    }

                    Vector3 spawnPos = Game.Player.Character.Position + Game.Player.Character.ForwardVector;
                    Vector3 rotation = Game.Player.Character.Rotation;
                    spawnedVehicle = World.CreateVehicle(vehicleHash, spawnPos);
                    spawnedVehicle.Rotation = rotation;
                    spawnedVehicle.PlaceOnGround();
                    spawnedVehicle.IsEngineRunning = true;
                    Notification.Show(spawnedVehicle.DisplayName + " spawned!");
                    Ped player = Game.Player.Character;
                    player.SetIntoVehicle(spawnedVehicle, VehicleSeat.Driver);
                };
                vehicleListMenu.Add(vehicleItem);
            }



            var backItem = new NativeItem("Back");
            backItem.Activated += (itemSender, itemEventArgs) => {
                // Go back to the main menu
                vehicleListMenu.Visible = false;
                menu.Visible = true;
            };
            vehicleListMenu.Add(backItem);

            menu.Visible = false;

            pool.Add(vehicleListMenu);
            vehicleListMenu.Visible = true;

            vehicleListMenu.HeldTime = 80;

        }
        #endregion
        
        #region OnGodModeChecked
        private void OnGodModeChecked(object sender, EventArgs e)
        {

            bool isChecked = vehicleGodModeItem.Checked;

            if (VehicleManager.Instance.CurrentVehicle != null)
            {

                if (isChecked)
                {
                    VehicleManager.Instance.CurrentVehicle.Repair();
                    VehicleManager.Instance.CurrentVehicle.IsInvincible = true;
                    Game.Player.Character.IsInvincible = true;
                    VehicleManager.Instance.CurrentVehicle.IsExplosionProof = true;
                    VehicleManager.Instance.CurrentVehicle.IsBulletProof = true;
                    VehicleManager.Instance.CurrentVehicle.IsMeleeProof = true;
                    VehicleManager.Instance.CurrentVehicle.IsFireProof = true;
                    VehicleManager.Instance.CurrentVehicle.IsCollisionProof = true;
                    VehicleManager.Instance.CurrentVehicle.CanBeVisiblyDamaged = false;
                    VehicleManager.Instance.CurrentVehicle.CanTiresBurst = false;
                    VehicleManager.Instance.CurrentVehicle.Driver.CanFlyThroughWindscreen = false;
                }

                else
                {
                    VehicleManager.Instance.CurrentVehicle.IsExplosionProof = false;
                    VehicleManager.Instance.CurrentVehicle.IsBulletProof = false;
                    if (!isGodMode)
                    {
                        Game.Player.Character.IsInvincible = false;
                    }

                    VehicleManager.Instance.CurrentVehicle.IsMeleeProof = false;
                    VehicleManager.Instance.CurrentVehicle.IsFireProof = false;
                    VehicleManager.Instance.CurrentVehicle.IsCollisionProof = false;
                    VehicleManager.Instance.CurrentVehicle.CanBeVisiblyDamaged = true;
                    VehicleManager.Instance.CurrentVehicle.IsInvincible = false;
                    VehicleManager.Instance.CurrentVehicle.CanTiresBurst = true;
                    VehicleManager.Instance.CurrentVehicle.Driver.CanFlyThroughWindscreen = true;
                }
            }
        }
        #endregion
        
        #region
        bool isGodMode;
        private void OnPlayerGodModeActivated(object sender, EventArgs e)
        {
             isGodMode= playerGodModeItem.Checked;
            if (isGodMode)
            {
                Game.Player.Character.IsInvincible = true;
            }
            else
            {
                Game.Player.Character.IsInvincible = false;
            }
        }
        #endregion

        #region OnLightsMultiplierActivated
        private void OnLightsMultiplierActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();
            
            int newLightsMultiplier;

            if(VehicleManager.Instance.CurrentVehicle != null)
            {
                if(int.TryParse(Game.GetUserInput(), out newLightsMultiplier))
                {
                    VehicleManager.Instance.CurrentVehicle.LightsMultiplier = newLightsMultiplier;
                    menu.Visible = false;
                }
                else
                {
                    OutputMessages.ShowInvalidInputMessage();
                }
            }

            else
            {
                OutputMessages.ShowInvalidInputMessage();
            }
        }
        #endregion

        #region OnVehicleMassActivated
        private void OnVehicleMassActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();

            int newMass;

            if (VehicleManager.Instance.CurrentVehicle != null)
            {
                if (int.TryParse(Game.GetUserInput(), out newMass))
                {
                    VehicleManager.Instance.CurrentVehicle.HandlingData.Mass = newMass;
                    
                    Notification.Show("Current Vehicle Weight in Kilograms: " + VehicleManager.Instance.CurrentVehicle.HandlingData.Mass.ToString());

                    menu.Visible = false;

                }
                else
                {
                    OutputMessages.ShowInvalidInputMessage();
                }
            }

            else
            {
                OutputMessages.ShowNotInVehicleMessage();
            }
        }
        #endregion

        #region ForceMenu
        private void OnForceMenuItemActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();

            menu.Visible = false;

            forceMenu.Visible = true;
            
            forceMenu.UseMouse = false;





        }

        public void OnForceMenuForceInputActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();

            if (float.TryParse(Game.GetUserInput(), NumberStyles.Float, CultureInfo.InvariantCulture, out forceVal))
            {


                // Add new menu item with updated force value

                forceInputItem.Description =forceVal.ToString();


                gotForce = true;
                
                forceMenuItemCounter++;

            }
            else
            {
                OutputMessages.ShowInvalidInputMessage();
            }

            

        }

        private void OnForceActivateCheck(object sender, EventArgs e)
        {

            
            if (VehicleManager.Instance.CurrentVehicle != null)
            {
                VehicleManager.Instance.CurrentVehicle.ApplyForce(VehicleManager.Instance.CurrentVehicle.ForwardVector * forceVal);
            }
            else if (VehicleManager.Instance.CurrentVehicle == null)
            {
                OutputMessages.ShowNotInVehicleMessage();
            }
            

        }
        #endregion

        #region OnVehicleStopActivated
        private void OnVehicleStopActivated(object sender, EventArgs e)
        {
           
            bool isChecked = vehicleStopCheckboxItem.Checked;

            if (isChecked)
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    if(VehicleManager.Instance.CurrentVehicle != vehicle)
                    {
                        vehicle.IsPositionFrozen = true;
                    }
                }

            }
            
            else
            {
                foreach (Vehicle vehicle in World.GetAllVehicles())
                {
                    vehicle.IsPositionFrozen = false;
                }
            }

        }
        #endregion
        
        #region OnTractionInputActivated
        private void OnTractionInputActivated(object sender, EventArgs e)
        {
            menu.SoundOpened?.PlayFrontend();

            int newTraction;

            if (VehicleManager.Instance.CurrentVehicle != null)
            {
                if (int.TryParse(Game.GetUserInput(), out newTraction))
                {
                    VehicleManager.Instance.CurrentVehicle.HandlingData.TractionCurveMax = newTraction;

                }
                else
                {
                    OutputMessages.ShowInvalidInputMessage();
                }
            }

            else
            {
                OutputMessages.ShowNotInVehicleMessage();
            }
        }
        #endregion

        #region OnAggressiveDriversActivated
        private void OnAggressiveDriversActivated(object sender, EventArgs e)
        {
            aggressiveDriversMode = aggressiveDriversCheckBoxItem.Checked;

            foreach (Vehicle vehicle in World.GetAllVehicles())
            {
                if (vehicle != null && vehicle.Driver != null && vehicle.Exists())
                {
                    if (aggressiveDriversMode)
                    {
                        vehicle.Driver.DrivingStyle = DrivingStyle.AvoidTrafficExtremely;
                    }
                    else
                    {
                        vehicle.Driver.DrivingStyle = DrivingStyle.Normal;


                    }
                }
            }

        }
        #endregion

        #region OnCarCanForceDrivable
        private void OnCarCanForceDrivable(object sender, EventArgs e)
        {
            if(VehicleManager.Instance.CurrentVehicle != null) 
            {
                carCanDrivableByForce = carCanForceDrivable.Checked;

            }
            else
            {
                OutputMessages.ShowNotInVehicleMessage();
            } 
        }
        #endregion
    }
}
