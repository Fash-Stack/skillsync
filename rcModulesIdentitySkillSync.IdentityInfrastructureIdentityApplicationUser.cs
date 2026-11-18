[1mdiff --git a/src/Modules/Identity/SkillSync.Identity/Infrastructure/IdentityDbContext.cs b/src/Modules/Identity/SkillSync.Identity/Infrastructure/IdentityDbContext.cs[m
[1mindex 19958cd..1e2f690 100644[m
[1m--- a/src/Modules/Identity/SkillSync.Identity/Infrastructure/IdentityDbContext.cs[m
[1m+++ b/src/Modules/Identity/SkillSync.Identity/Infrastructure/IdentityDbContext.cs[m
[36m@@ -1,7 +1,24 @@[m
[31m-﻿using Microsoft.EntityFrameworkCore;[m
[32m+[m[32m﻿using Microsoft.AspNetCore.Identity;[m
[32m+[m[32musing Microsoft.AspNetCore.Identity.EntityFrameworkCore;[m
[32m+[m[32musing Microsoft.EntityFrameworkCore;[m
[32m+[m
[32m+[m[32musing SkillSync.Identity.Infrastructure.Identity;[m
 [m
 namespace SkillSync.Identity.Infrastructure;[m
 [m
[31m-internal class IdentityDbContext : DbContext[m
[32m+[m[32minternal class IdentityDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>[m
 {[m
[32m+[m[32m    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)[m
[32m+[m[32m        : base(options)[m
[32m+[m[32m    {[m
[32m+[m[41m        [m
[32m+[m[32m    }[m
[32m+[m
[32m+[m[32m    protected override void OnModelCreating(ModelBuilder modelBuilder)[m
[32m+[m[32m    {[m
[32m+[m[32m        modelBuilder.HasDefaultSchema("Identity");[m
[32m+[m
[32m+[m[32m        base.OnModelCreating(modelBuilder);[m
[32m+[m[32m    }[m
 }[m
[41m+[m
[1mdiff --git a/src/Modules/Identity/SkillSync.Identity/SkillSync.Identity.csproj b/src/Modules/Identity/SkillSync.Identity/SkillSync.Identity.csproj[m
[1mindex f2bb38a..9da6762 100644[m
[1m--- a/src/Modules/Identity/SkillSync.Identity/SkillSync.Identity.csproj[m
[1m+++ b/src/Modules/Identity/SkillSync.Identity/SkillSync.Identity.csproj[m
[36m@@ -6,6 +6,7 @@[m
   </ItemGroup>[m
 [m
   <ItemGroup>[m
[32m+[m[32m    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.3" />[m
     <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.0.3" />[m
   </ItemGroup>[m
 [m
