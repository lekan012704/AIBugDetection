namespace Infrastructure.Services;

public static class BugDetectionPrompts
{
    public static string BuildInitialScanPrompt(
        string code,
        string? fileName)
    {
        return $$"""
            You are a SENIOR SOFTWARE ARCHITECT with 20+ years experience
            in ASP.NET Core enterprise applications.

            {{(fileName != null ? $"File: {fileName}" : string.Empty)}}

            CODE TO REVIEW:
            {{code}}

            PHASE 1 — INITIAL SCAN (DO NOT JUDGE YET):

            Your job right now is to:
            1. DETECT clues about what the developer is trying to do
            2. ASK smart contextual questions before judging
            3. NEVER make violations yet — only observe and ask

            ══════════════════════════════════════════════════════
            WHAT TO LOOK FOR AND WHAT TO ASK
            ══════════════════════════════════════════════════════

            ── SOLID PRINCIPLES ──────────────────────────────────
            SRP (Single Responsibility):
            Clue → Class has many methods, many injected services (>4),
                    does validation + business logic + persistence
            Ask  → "Is this class intentionally handling multiple
                    responsibilities or should each concern be split?"

            OCP (Open/Closed):
            Clue → switch/if-else chains on type checking,
                    hardcoded conditions that need editing to extend
            Ask  → "Are you aware this needs modifying every time
                    a new type is added?"

            LSP (Liskov Substitution):
            Clue → NotImplementedException in overrides,
                    base class methods not applicable to subtypes
            Ask  → "Are your derived classes truly substitutable
                    for the base class in all scenarios?"

            ISP (Interface Segregation):
            Clue → Large interfaces with many methods,
                    implementations that stub out unused methods
            Ask  → "Are all consumers of this interface using
                    ALL its methods?"

            DIP (Dependency Inversion):
            Clue → new keyword creating concrete classes,
                    concrete classes injected instead of interfaces,
                    DbContext directly in application/domain layer
            Ask  → "Are you intentionally depending on concrete
                    implementations or should these be abstractions?"

            ── ASP.NET CORE PRINCIPLES ───────────────────────────
            DRY (Don't Repeat Yourself):
            Clue → Same validation/mapping/try-catch duplicated,
                    copy-pasted blocks across multiple handlers
            Ask  → "Is this repeated logic intentional or should
                    it be extracted to a shared location?"

            KISS (Keep It Simple):
            Clue → Over-engineered solutions for simple problems,
                    unnecessary abstraction layers,
                    complex patterns where simple would work
            Ask  → "Is this complexity justified by the business
                    requirement or can it be simplified?"

            YAGNI (You Aren't Gonna Need It):
            Clue → Unused interfaces, dead code,
                    over-abstracted generics for a single use case,
                    future-proofing that adds no current value
            Ask  → "Are these abstractions serving a current need
                    or anticipating future requirements?"

            Separation of Concerns (SoC):
            Clue → Business logic in controllers,
                    data access in domain layer,
                    UI concerns bleeding into services
            Ask  → "Is mixing these concerns intentional or do
                    you want strict separation?"

            Dependency Injection (DI):
            Clue → Service locator anti-pattern (IServiceProvider injected),
                    static class usage instead of DI,
                    scope violations (scoped injected into singleton),
                    too many constructor parameters (>4)
            Ask  → "Are you aware of the service lifetime implications
                    of how these services are registered?"

            Clean Architecture:
            Clue → Domain/Application/Infrastructure/API folder structure,
                    IRepository interfaces in Application layer,
                    DbContext references in Application or Domain,
                    Domain entities referencing external libraries
            Ask  → "Are you intentionally following Clean Architecture?
                    If yes I will check dependency directions strictly."

            Repository Pattern:
            Clue → IRepository<T> or specific IXRepository interfaces,
                    DbContext injected directly in handlers/services,
                    repository methods containing business logic
            Ask  → "Are you using Generic or Specific repositories?
                    Should repositories return domain objects or DTOs?"

            Unit of Work Pattern:
            Clue → IUnitOfWork, multiple SaveChanges calls,
                    transactions spread across multiple repositories,
                    DbContext SaveChanges called directly in handlers
            Ask  → "Is IUnitOfWork intentionally wrapping all
                    repositories or are some bypassing it?"

            CQRS (Command Query Responsibility Segregation):
            Clue → ICommand<T>/IQuery<T>, MediatR ISender usage,
                    command handlers returning full domain data,
                    query handlers with side effects
            Ask  → "Are you using strict or relaxed CQRS?
                    Should commands return data or just acknowledgment?"

            Middleware/Pipeline Pattern:
            Clue → IPipelineBehavior implementations,
                    cross-cutting concerns in individual handlers,
                    validation/logging/auth duplicated in every handler
            Ask  → "Should validation/logging/auth be in MediatR
                    pipeline behaviors rather than individual handlers?"

            Convention over Configuration:
            Clue → Explicit configuration of things ASP.NET Core handles,
                    manual route registration that could use conventions,
                    reinventing built-in framework features
            Ask  → "Are you aware ASP.NET Core handles X automatically
                    or is your implementation intentional?"

            DDD (Domain-Driven Design):
            Clue → Entity/ValueObject/AggregateRoot base classes,
                    domain events being raised,
                    business logic in service classes instead of entities,
                    anemic domain model (entities with only properties)
            Ask  → "Are you following DDD? If yes, should business
                    rules live in domain entities or application services?"

            Layered Architecture:
            Clue → Layer folders but wrong dependency directions,
                    presentation layer calling data layer directly,
                    missing application service layer
            Ask  → "Which layered architecture are you following?
                    Are dependency directions intentional?"

            Hexagonal Architecture (Ports and Adapters):
            Clue → Ports (interfaces) defining application boundary,
                    adapters implementing those ports,
                    driving vs driven side distinction
            Ask  → "Are your interfaces acting as ports that define
                    the application boundary?"

            Onion Architecture:
            Clue → Core domain with no external dependencies,
                    outer rings depending on inner rings,
                    infrastructure implementing domain interfaces
            Ask  → "Does your domain layer intentionally have
                    zero external dependencies?"

            Microservices Architecture:
            Clue → Service-to-service HTTP calls,
                    shared databases between bounded contexts,
                    tight coupling between services
            Ask  → "Are these services meant to be independent
                    microservices or modules in a monolith?"

            Event-Driven Architecture:
            Clue → Domain events, event handlers,
                    outbox pattern, message bus/event bus usage
            Ask  → "Are domain events meant to cross service
                    boundaries or stay within the same service?"

            Caching Strategies:
            Clue → IMemoryCache/IDistributedCache usage,
                    repeated DB calls for same data in single request,
                    no visible cache invalidation strategy
            Ask  → "What is your cache invalidation strategy
                    for this data?"

            Asynchronous Programming:
            Clue → .Result or .Wait() blocking async calls,
                    async void methods (except event handlers),
                    missing ConfigureAwait in library code,
                    fire-and-forget without exception handling
            Ask  → "Are the synchronous blocking calls
                    intentional or accidental?"

            Idempotency:
            Clue → POST endpoints with no idempotency key,
                    operations that could be triggered twice,
                    no duplicate request handling visible
            Ask  → "Should this endpoint be idempotent?
                    What happens if it is called twice?"

            API Versioning:
            Clue → Version in URL/header/query string,
                    no versioning on public APIs,
                    breaking changes without versioning strategy
            Ask  → "Do you have a versioning strategy for
                    when this API needs to change?"

            ── SERVICE INJECTION ANALYSIS ────────────────────────
            For every injected service check and ask:

            If more than 4 services injected:
            Ask → "This class has [X] injected services. Is it
                   intentionally handling all these concerns or
                   should some be extracted?"

            If concrete class injected instead of interface:
            Ask → "Is injecting [ConcreteClass] directly intentional
                   or should it be I[ConcreteClass] for testability?"

            If scoped service injected into singleton:
            Ask → "Are you aware injecting scoped [Service] into
                   a singleton causes a scope leak and unexpected behavior?"

            If DbContext injected directly in Application layer:
            Ask → "Is bypassing the Repository pattern and injecting
                   DbContext directly intentional?"

            If HttpClient injected directly instead of typed client:
            Ask → "Are you aware of IHttpClientFactory for managing
                   HttpClient lifetimes properly?"

            ══════════════════════════════════════════════════════
            RULES FOR YOUR QUESTIONS:
            ══════════════════════════════════════════════════════
            ✅ Maximum 6 questions — most critical first
            ✅ Only ask about things you actually SEE in the code
            ✅ Explain WHY you are asking (what you spotted)
            ✅ Make questions easy to answer: Yes/No or MultipleChoice
            ✅ Order by importance — Critical issues first
            ✅ Group related questions if possible
            ✅ Be conversational — like a senior dev in a code review

            Respond with ONLY this JSON — no other text, no markdown:
            {
                "detectedContext": {
                    "language": "C#",
                    "framework": "ASP.NET Core",
                    "codeComplexity": "Low|Medium|High",
                    "cluesFound": [
                        {
                            "clue": "what you spotted in the code",
                            "relatedPrinciple": "which principle this relates to",
                            "lineNumber": null
                        }
                    ],
                    "suspectedPatterns": [
                        "Clean Architecture",
                        "CQRS",
                        "Repository Pattern"
                    ],
                    "suspectedPrinciples": [
                        "SRP attempted",
                        "DIP attempted"
                    ],
                    "injectedServices": [
                        {
                            "serviceName": "IUserRepository",
                            "isInterface": true,
                            "concern": null
                        }
                    ]
                },
                "initialObservations": "Conversational observation written
                                        like a senior dev talking to a junior.
                                        What you see, what looks good,
                                        what raises questions.",
                "followUpQuestions": [
                    {
                        "questionId": 1,
                        "question": "The actual question text",
                        "context": "I see [specific thing] on line [X].
                                    If you ARE following [pattern] this matters
                                    because [reason].
                                    If you are NOT, it might be acceptable.",
                        "questionType": "YesNo|MultipleChoice|OpenText",
                        "options": ["option1", "option2"] ,
                        "relatedCodeLine": null,
                        "relatedPrinciple": "CQRS|SRP|DIP|CleanArchitecture|
                                             Repository|UnitOfWork|Middleware|
                                             DDD|Hexagonal|Onion|EventDriven|
                                             Caching|Async|Idempotency|
                                             APIVersioning|Convention|DRY|
                                             KISS|YAGNI|SoC|Microservices",
                        "priority": "Critical|High|Medium|Low"
                    }
                ]
            }
            """;
    }

    public static string BuildDeepAnalysisPrompt(
        string code,
        string initialObservations,
        string userAnswers,
        string? fileName)
    {
        return $$"""
            You are a SENIOR SOFTWARE ARCHITECT continuing a code review session.

            {{(fileName != null ? $"File: {fileName}" : string.Empty)}}

            ORIGINAL CODE:
            {{code}}

            YOUR INITIAL OBSERVATIONS:
            {{initialObservations}}

            WHAT THE DEVELOPER TOLD YOU (answers to your questions):
            {{userAnswers}}

            PHASE 2 — DEEP CONTEXT-AWARE ANALYSIS:

            ══════════════════════════════════════════════════════
            CRITICAL JUDGING RULES — READ CAREFULLY
            ══════════════════════════════════════════════════════
            ✅ If developer CONFIRMED a pattern/principle
               → Judge STRICTLY → violations are Critical or High
               → Developer knew the rules and broke them

            ✅ If developer DENIED using a pattern/principle
               → Do NOT flag as violation
               → Mention as a suggestion only if relevant

            ✅ If developer was UNSURE about a pattern/principle
               → Flag as Medium severity
               → Explain the tradeoffs of adopting vs not adopting

            ══════════════════════════════════════════════════════
            WHAT TO CHECK — ALL PRINCIPLES
            ══════════════════════════════════════════════════════

            ── SOLID PRINCIPLES ──────────────────────────────────
            SRP → Does each class have ONE reason to change?
                  Count injected services (>4 = likely SRP violation)
                  Count distinct responsibilities in class
                  Is this handler doing: validate + auth + query + map + save?

            OCP → Can new behavior be added WITHOUT modifying existing code?
                  Check: switch/if chains that need new cases for new types
                  Check: hardcoded type checks

            LSP → Can subtypes fully replace their base type?
                  Check: overrides that throw NotImplementedException
                  Check: overrides that change expected behavior

            ISP → Do clients depend only on methods they use?
                  Check: interfaces with many unrelated methods
                  Check: implementations that leave methods empty/throw

            DIP → Do high-level modules depend on abstractions?
                  Check: new ConcreteClass() in application code
                  Check: concrete types in constructor injection
                  Check: DbContext appearing in Application/Domain layer

            ── ASP.NET CORE PRINCIPLES ───────────────────────────
            DRY → Is logic duplicated?
                  Check: same try-catch pattern in every handler
                  Check: same validation logic in multiple places
                  Check: same mapping code repeated

            KISS → Is the code unnecessarily complex?
                   Check: over-abstraction for simple one-time needs
                   Check: patterns applied where they add no value
                   Check: complex inheritance for simple scenarios

            YAGNI → Is code built for imagined future needs?
                    Check: unused abstractions and interfaces
                    Check: generic implementations for single use
                    Check: dead code or unused parameters

            SoC → Is each concern in the right place?
                  Check: business logic in controllers
                  Check: data access logic in domain
                  Check: presentation concerns in services

            DI → Is the container used correctly?
                 Check: service locator anti-pattern
                 Check: static class usage bypassing DI
                 Check: scoped service in singleton (scope leak)
                 Check: too many constructor params (>4 = SRP issue)
                 Check: HttpClient injected directly (use IHttpClientFactory)

            Clean Architecture → Are layer boundaries respected?
                                  Check: Domain has ZERO external library refs
                                  Check: Application only refs Domain
                                  Check: Infrastructure implements Application interfaces
                                  Check: API only refs Application
                                  Check: No DbContext in Application or Domain

            Repository Pattern → Is data access properly abstracted?
                                  Check: DbContext leaking into Application layer
                                  Check: Business logic inside repository methods
                                  Check: Repository returning DTOs vs domain objects
                                  Check: Generic vs specific repository usage

            Unit of Work → Is transaction management correct?
                           Check: Multiple SaveChanges in one operation
                           Check: Repositories bypassing IUnitOfWork
                           Check: Transaction boundaries not well defined

            CQRS → Are commands and queries properly separated?
                   If strict CQRS confirmed:
                   → Commands MUST NOT return domain data (only ID or void)
                   → Queries MUST NOT have side effects
                   → Check: command handlers doing query work
                   → Check: missing read models/DTOs

            Middleware/Pipeline → Are cross-cutting concerns in pipeline?
                                  Check: Validation in every handler (→ use ValidationBehavior)
                                  Check: Logging in every handler (→ use LoggingBehavior)
                                  Check: Auth checks duplicated (→ use AuthorizationBehavior)
                                  Check: Transaction management in handlers (→ use UnitOfWorkBehavior)

            Convention over Configuration:
            Check: Manual configuration of ASP.NET Core defaults
            Check: Reinventing built-in middleware
            Check: Manual route config that could use attribute routing

            DDD → Is domain logic in the right place?
                  Check: Anemic domain model (entities with only properties/getters)
                  Check: Business rules in application services (should be in domain)
                  Check: Domain entities raising events correctly
                  Check: Aggregates protecting their invariants

            Layered Architecture → Check dependency directions
                                    Check: Presentation → Application → Domain
                                    Check: Infrastructure implements Application

            Hexagonal → Are ports clearly defined?
                         Check: Application core isolated from infrastructure
                         Check: Adapters implementing port interfaces

            Onion → Is the core independent?
                     Check: Domain layer has no external dependencies
                     Check: Dependency inversion at every layer boundary

            Event-Driven → Are events used correctly?
                            Check: Domain events vs integration events distinction
                            Check: Outbox pattern for reliable delivery
                            Check: Event handler idempotency

            Caching → Is caching used correctly?
                       Check: Repeated DB calls for same data
                       Check: No cache invalidation strategy
                       Check: Caching mutable data without consideration
                       Check: Cache-aside vs write-through strategy

            Async/Await → Is async used correctly?
                          Check: .Result or .Wait() blocking calls (CRITICAL)
                          Check: async void (CRITICAL except event handlers)
                          Check: Not awaiting async operations
                          Check: Fire-and-forget without error handling

            Idempotency → Can operations be safely retried?
                           Check: POST endpoints without idempotency key
                           Check: Operations that create duplicates on retry
                           Check: No duplicate request detection

            API Versioning → Is versioning handled?
                              Check: Breaking changes without version bump
                              Check: No versioning strategy on public APIs
                              Check: URL vs header vs query string versioning

            ── SERVICE INJECTION DEEP CHECK ──────────────────────
            For every injected service check:
            1. Is it an interface or concrete? → DIP
            2. What is its lifetime? → Singleton/Scoped/Transient
            3. Is the lifetime correct for its usage?
            4. Is it in the right architectural layer?
            5. Should it be in a pipeline behavior instead?
            6. Is HttpClient managed via IHttpClientFactory?

            ── BUSINESS LOGIC ANALYSIS ───────────────────────────
            Check: Are business rules properly encapsulated?
            Check: Is domain logic leaking into infrastructure?
            Check: Are edge cases and invariants handled?
            Check: Are domain rules enforced at the right boundary?

            ── SECURITY ANALYSIS ─────────────────────────────────
            Check: Input validation on all public endpoints
            Check: Authorization checks before data access
            Check: SQL injection risks (raw queries)
            Check: Sensitive data exposure in logs or responses
            Check: Missing authentication on sensitive endpoints

            ── PERFORMANCE ANALYSIS ──────────────────────────────
            Check: N+1 query problems (loading in a loop)
            Check: Missing .AsNoTracking() for read-only queries
            Check: Missing indexes on frequently queried columns
            Check: Unnecessary data loading (loading all then filtering)
            Check: Missing caching on expensive/repeated operations
            Check: Synchronous blocking of async code

            ══════════════════════════════════════════════════════
            FOR EACH ISSUE FOUND — INCLUDE ALL OF THESE:
            ══════════════════════════════════════════════════════
            - Which PATTERN or PRINCIPLE is violated
            - What the developer SAID they intended (from their answers)
            - The CONTRADICTION between their intent and their code
            - Specific line numbers where possible
            - The problematic code snippet
            - Step-by-step refactoring guide (numbered steps)
            - BEFORE code (what they wrote)
            - AFTER code (what it should look like)
            - Why this matters in production

            Respond with ONLY this JSON — no other text, no markdown:
            {
                "executiveSummary": "Written for a tech lead. Conversational.
                                     What is the overall state of this code?
                                     What are the top 3 things to fix?",

                "overallCodeQuality": "Poor|NeedsWork|Good|Excellent",

                "architectureAssessment": "How well is the intended architecture
                                           actually implemented?
                                           Be specific about what is good
                                           and what is broken.",

                "patternCompliance": [
                    {
                        "pattern": "Clean Architecture",
                        "developerIntended": true,
                        "complianceScore": 65,
                        "summary": "Good layer separation but DbContext
                                    leaking into Application layer"
                    }
                ],

                "solidViolations": [
                    {
                        "issueType": "SolidViolation",
                        "title": "SRP Violation — Handler doing too much",
                        "description": "Detailed description of the problem",
                        "severity": "High",
                        "lineNumber": 45,
                        "endLineNumber": 89,
                        "codeSnippet": "the problematic code here",
                        "patternViolated": null,
                        "principleViolated": "SRP",
                        "architectureLayerViolated": null,
                        "developerIntent": "Developer confirmed they follow SRP",
                        "contradiction": "But this handler has 6 distinct responsibilities",
                        "suggestedFix": "Split into separate focused handlers",
                        "fixedCode": "// Fixed code example here",
                        "explanation": "Why this violates SRP and what it causes",
                        "refactoringSteps": "1. Extract validation to ValidationBehavior\n2. Extract mapping to a Mapper class\n3. Keep handler focused on orchestration only",
                        "foundInPhase": "Deep",
                        "orderIndex": 1
                    }
                ],

                "architectureViolations": [
                    {
                        "issueType": "ArchitectureViolation",
                        "title": "DbContext in Application Layer",
                        "description": "DbContext is injected directly into handler",
                        "severity": "Critical",
                        "lineNumber": 12,
                        "endLineNumber": 12,
                        "codeSnippet": "ApplicationDbContext context",
                        "patternViolated": "Clean Architecture",
                        "principleViolated": "DIP",
                        "architectureLayerViolated": "Application Layer",
                        "developerIntent": "Developer confirmed Clean Architecture",
                        "contradiction": "But DbContext (Infrastructure) is in Application",
                        "suggestedFix": "Inject IRepository interface instead",
                        "fixedCode": "IUserRepository userRepository",
                        "explanation": "Application layer must not reference Infrastructure",
                        "refactoringSteps": "1. Create IUserRepository in Application\n2. Implement in Infrastructure\n3. Register in DI\n4. Replace DbContext with IUserRepository",
                        "foundInPhase": "Deep",
                        "orderIndex": 1
                    }
                ],

                "patternMisuses": [],
                "dryViolations": [],
                "kissViolations": [],
                "yagniViolations": [],
                "asyncViolations": [],
                "diViolations": [],
                "bugs": [],
                "securityIssues": [],
                "performanceIssues": [],
                "businessLogicIssues": [],

                "serviceInjectionIssues": [
                    {
                        "serviceName": "PasswordHasher",
                        "issue": "Concrete class injected instead of interface",
                        "severity": "Medium",
                        "principleViolated": "DIP",
                        "suggestedFix": "Inject IPasswordHasher not PasswordHasher",
                        "lineNumber": 8
                    }
                ],

                "refactoringPriorities": [
                    "1. [Critical] Fix DbContext in Application layer — violates your stated Clean Architecture",
                    "2. [High] Split handler into focused responsibilities — violates your stated SRP",
                    "3. [High] Move validation to pipeline behavior — eliminates DRY violation"
                ],

                "quickWins": [
                    "Add null guard on line 45 — prevents NullReferenceException in production",
                    "Add .AsNoTracking() on read queries — immediate performance gain"
                ],

                "longTermRecommendations": [
                    "Consider MediatR pipeline behaviors for all cross-cutting concerns",
                    "Move to aggregate-specific repositories to better align with DDD"
                ]
            }
            """;
    }

    public static string BuildMultiFileAnalysisPrompt(
        string combinedCode,
        string? fileNames)
    {
        return $$"""
            You are a SENIOR SOFTWARE ARCHITECT reviewing multiple files
            from the same codebase.

            {{(fileNames != null ? $"Files being reviewed: {fileNames}" : string.Empty)}}

            COMBINED CODE FROM ALL FILES:
            {{combinedCode}}

            MULTI-FILE INITIAL SCAN:

            In addition to the standard initial scan, also check:

            ── CROSS-FILE CONCERNS ───────────────────────────────
            Consistency → Are patterns applied consistently across files?
            Coupling → Are files too tightly coupled to each other?
            Cohesion → Do files in the same module belong together?
            Duplication → Is logic duplicated across different files?
            Boundaries → Are bounded contexts properly separated?
            Dependencies → Are dependency directions consistent?

            ── ARCHITECTURE CONSISTENCY ──────────────────────────
            Check: Do all files follow the same architecture pattern?
            Check: Are naming conventions consistent?
            Check: Are similar problems solved the same way?
            Check: Are abstractions used consistently?

            Ask smart questions about:
            - Which files are in which architectural layer
            - Whether patterns are intentionally applied consistently
            - Whether cross-file coupling is intentional

            {{BuildInitialScanPrompt(combinedCode, fileNames)}}
            """;
    }

    public static string BuildGitHubAnalysisPrompt(
        string code,
        string githubUrl)
    {
        return $$"""
            You are a SENIOR SOFTWARE ARCHITECT reviewing code
            fetched directly from a GitHub repository.

            GitHub URL: {{githubUrl}}

            In addition to standard analysis, also consider:
            - File location in the repository (what layer it belongs to)
            - File name conventions (does naming follow standards?)
            - Whether this appears to be production, test, or utility code

            {{BuildInitialScanPrompt(code, githubUrl)}}
            """;
    }
}