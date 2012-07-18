Mongo support for MiniProfiler
See the home page at: http://miniprofiler.com

To profile, do the following when you create your database:

    new ProfiledMongoDatabase(server, dbSettings, MiniProfiler.Current);
    
And make sure to set the Mongo formatter

    MiniProfiler.Settings.MongoFormatter = new MongoFormatter();

Licensed under apache 2.0 license, see: http://www.apache.org/licenses/LICENSE-2.0

