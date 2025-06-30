using RZ.Foundation.MongoDb;

namespace RZ.Foundation.Audit.Mongo;

public class MongoAuditLogDbContext(string connectionString) : RzMongoDbContext(connectionString);