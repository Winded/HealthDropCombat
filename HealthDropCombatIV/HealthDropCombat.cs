using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthDropCombatIV
{
    public class HealthDropCombat : Script
    {
        public readonly static RelationshipGroup[] HostilePedGroups = new RelationshipGroup[]
        {
            RelationshipGroup.Cop,
            RelationshipGroup.Gang_Albanian,
            RelationshipGroup.Gang_Biker1,
            RelationshipGroup.Gang_Biker2,
            RelationshipGroup.Gang_Italian,
            RelationshipGroup.Gang_Russian1,
            RelationshipGroup.Gang_Russian2,
            RelationshipGroup.Gang_Irish,
            RelationshipGroup.Gang_Jamaican,
            RelationshipGroup.Gang_AfricanAmerican,
            RelationshipGroup.Gang_Korean,
            RelationshipGroup.Gang_ChineseJapanese,
            RelationshipGroup.Gang_PuertoRican,
            RelationshipGroup.Dealer,
            RelationshipGroup.Criminal,
            RelationshipGroup.Bum,
            RelationshipGroup.Special,
            RelationshipGroup.Mission_1,
            RelationshipGroup.Mission_2,
            RelationshipGroup.Mission_3,
            RelationshipGroup.Mission_4,
            RelationshipGroup.Mission_5,
            RelationshipGroup.Mission_6,
            RelationshipGroup.Mission_7,
            RelationshipGroup.Mission_8,
        };

        public readonly int GreenLightHash = 0x6A299B19;

        public HealthDropCombat()
        {
            Interval = 50;
            Tick += OnTick;
            KeyDown += OnKeyDown;
            // Immediately get first peds
            _PedListUpdateTimer = _PedListUpdateInterval;
            Player.MaxHealth = _PlayerMaxHealth;
            Player.Character.Health = _PlayerMaxHealth;
        }

        private int _PlayerMaxHealth = 100;

        private int _HealthPerPickup = 25;

        private float _PickupDistance = 3f;
        private float _PickupDeleteDistance = 500f;

        private bool _RequireCompleteDeath = false;

        // Update ped list every 5 seconds
        private int _PedListUpdateInterval = 5000;
        private int _PedListUpdateTimer = 0;

        private List<Ped> _TrackedPeds = new List<Ped>();

        private List<GTA.Object> _Pickups = new List<GTA.Object>();

        private void OnTick(object sender, EventArgs e)
        {
            TickPedList();
            TickCreatePickups();
            TickProcessPickups();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == System.Windows.Forms.Keys.NumPad1)
            {
                Game.DisplayText("Health: " + Player.Character.Health);
            }
        }

        private void TickPedList()
        {
            _PedListUpdateTimer += Interval;
            if (_PedListUpdateTimer >= _PedListUpdateInterval)
            {
                _TrackedPeds.Clear();
                var peds = World.GetAllPeds();
                foreach (var ped in peds)
                {
                    if (!ped.Exists()) continue;

                    bool isNotDead = _RequireCompleteDeath ? ped.isAlive : ped.Health > 0;
                    if (isNotDead && HostilePedGroups.Contains(ped.RelationshipGroup))
                    {
                        _TrackedPeds.Add(ped);
                    }
                }
                _PedListUpdateTimer = 0;
            }
        }

        private void TickCreatePickups()
        {
            foreach (var ped in _TrackedPeds)
            {
                if (!ped.Exists()) continue;

                // Ped died; spawn health pickup
                if ((_RequireCompleteDeath && ped.isDead) || (!_RequireCompleteDeath && ped.Health <= 0))
                {
                    var pickup = World.CreateObject(new Model("CJ_FIRST_AID_PICKUP"), ped.Position.ToGround());
                    pickup.Collision = false;
                    _Pickups.Add(pickup);
                }
            }

            // Remove dead or non-existing peds
            _TrackedPeds.RemoveAll(p => !p.Exists() || ((_RequireCompleteDeath && p.isDead) || (!_RequireCompleteDeath && p.Health <= 0)));
        }

        private void TickProcessPickups()
        {
            foreach (var pickup in _Pickups)
            {
                if (pickup.Exists() && pickup.Position.DistanceTo(Player.Character.Position) < _PickupDistance)
                {
                    Player.Character.Health += _HealthPerPickup;
                    Game.PlayFrontendSound("FRONTEND_GAME_PICKUP_HEALTH");
                    DeletePickup(pickup);
                }
                else if(pickup.Exists() && pickup.Position.DistanceTo(Player.Character.Position) >= _PickupDeleteDistance)
                {
                    DeletePickup(pickup);
                }
            }

            // Remove picked up or non-existing pickups
            _Pickups.RemoveAll(p => !p.Exists());
        }

        private void DeletePickup(GTA.Object pickup)
        {
            pickup.Delete();
        }
    }
}
