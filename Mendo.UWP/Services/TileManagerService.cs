using NotificationsExtensions;
using NotificationsExtensions.TileContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;

namespace Mendo.UWP.Services
{
    public sealed class TileManagerService
    {
        public static bool TilesCreated
        {
            get
            {
                ApplicationDataContainer appDataContainer = ApplicationData.Current.LocalSettings;
                if (appDataContainer.Values.ContainsKey(TilesCreatedKey))
                    return (bool)appDataContainer.Values[TilesCreatedKey];
                else
                    return false;
            }
            set
            {
                ApplicationDataContainer appDataContainer = ApplicationData.Current.LocalSettings;
                if (appDataContainer.Values.ContainsKey(TilesCreatedKey))
                    appDataContainer.Values[TilesCreatedKey] = value;
                else
                    appDataContainer.Values.Add(TilesCreatedKey, value);
            }
        }

        private static string TilesCreatedKey = "TilesCreated";

        public static void CreateTilesAsync()
        {
            Task.Run(() => CreateTiles());
        }

        public static void CreateTiles()
        {
            //var largeTiles = new List<ITileSquare310x310Image>(letters.Select(t => CreateLargeTile(string.Format("ms-appx:///Assets/TileImages/Large-Tile-Live-{0}.jpg", t))));
            //var wideTiles = new List<ITileWide310x150Image>(letters.Select(t => CreateWideTile(string.Format("ms-appx:///Assets/TileImages/Wide-Tile-Live-{0}.jpg", t))));
            //var mediumTiles = new List<ITileSquare150x150Image>(letters.Select(t => CreateMediumTile(string.Format("ms-appx:///Assets/TileImages/Square-Tile-Live-{0}.jpg", t))));

            //for (int i = 0; i < letters.Count(); i++)
            //{
            //    wideTiles[i].Square150x150Content = mediumTiles[i];
            //    largeTiles[i].Wide310x150Content = wideTiles[i];
            //}

            //var updater = TileUpdateManager.CreateTileUpdaterForApplication();

            //updater.EnableNotificationQueue(true);
            //updater.Clear();
            //foreach (var tile in largeTiles)
            //{
            //    updater.Update(tile.CreateNotification());
            //}

            //TilesCreated = true;
        }

        private static ITileSquare310x310Image CreateLargeTile(string imageUri)
        {
            var large = TileContentFactory.CreateTileSquare310x310Image();
            large.Image.Src = imageUri;
            large.Branding = TileBranding.None;
            large.RequireWide310x150Content = true;

            return large;
        }

        private static ITileWide310x150Image CreateWideTile(string imageUri)
        {
            var wide = TileContentFactory.CreateTileWide310x150Image();
            wide.Image.Src = imageUri;
            wide.Branding = TileBranding.None;
            wide.RequireSquare150x150Content = true;

            return wide;
        }

        private static ITileSquare150x150Image CreateMediumTile(string imageUri)
        {
            var square = TileContentFactory.CreateTileSquare150x150Image();
            square.Image.Src = imageUri;

            return square;
        }

        private static void AddTileToUpdateManager(XmlDocument tileXml)
        {
            TileNotification tileNotification = new TileNotification(tileXml);
            tileNotification.ExpirationTime = DateTimeOffset.Now.AddYears(100);
            var upd = TileUpdateManager.CreateTileUpdaterForApplication();
            upd.EnableNotificationQueue(true);
            upd.Update(tileNotification);
        }

    }
}
