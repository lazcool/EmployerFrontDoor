# Local Store Strategy

## What do we need to store in the cache?
Content associated with content types, currently Scheme, Filter Aspect. We should be able to store further content types going forward.

## In what format do we store the content?
Options:

a) Contentful's json representation of the content item.

b) a transformed format, e.g. GDS HTML snippet

c) store both contentful's json representation and the transformed format

### Option A
Pros:
* cache content is not front end channel specific, which would allow us to add other channels without rearchitecting, but extremely low likelihood as a future requirement

Cons:
* transformation would need to occur in each app service instance on every fetch from the cache, although it would be done in the background intermittently

### Option B
Pros:
* content is only transformed once during cache population

Cons:
* source content representation not available from cache for other uses

### Option C
Pros:
* support additional consumers of the content
* no redundant transforming of content, transforming is done once per cache population
* stores record of pre-transformed content associated with transformed content, useful for resolving issues

Cons:
* requires more space, but only a very small amount of content
* no consumers of Contentful's json representation, so why store it

### Conclusion
Due to the low volume of data, the immediate available of the tranformed content to the app service instances, the use in helping us resolving issues and giving us the most flexibility going forward, we should store both the source content representation  and the transformed content in the cache.

## What do we need to store?

### Scheme
* Scheme ID (from contententful)
* Scheme Contentful Representation (json)
* Search Aspect Contentful Representation (json)
* Scheme Description GDS Html Snippet, etc. as per content type schema
* Collection of: Search Aspect ID (from Contentful) + Enabled/Disabled

### Search Aspect
* Search Aspect Id
* Search Aspect Contentful Representation (json)
* Description GDS HTML

## How do we need to access the data?
The initial plan calls for always writing and fetching all the (transformed) data.
Do we want to support queries such as fetch e.g. a scheme by it's name or id? Not required, but a bonus if it's supported.

Infrequent writes (on deploy, rare content updates, (nightly?) infrequent backup writes
Infrequent reads -- full read of all content every 30 (configurable) mins per app service instance

All data to be replaced at once, so we require transaction support. The transaction doesn't need to extend outside the cache, so we don't require distributed transactions.

Open question: Does the data need to be available to datamart / data warehouse?

## How fast / scalable does it need to be?
Write speed not a concern.
Read speed not a concern, as the read by the app services will be done in the background and won't affect page response time (page responses will be served from an in memory root aggregate object)

## What should we use to store the local store?
After briefly considering no-sql solutions, such as Redis, CosmosDB and a graph database, which have their pros and cons, it was decided to use Azure Sql, due to:
* it'll work for what we need
* developer and devops familiarity
* as speed is not a concern due to the architecture, and there is only a small amount of data to store, we can go with a cheap tier e.g. S0, which costs ~£10 a month to host

## How will we talk to the SQL instance?
To keep everything simple, and not have to write or maintain any T-SQL, we'll use Entity Framework Core.