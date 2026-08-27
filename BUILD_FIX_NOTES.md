# Build Fix Notes

This version fixes the reported build errors:

- `TaskStatus` is explicitly aliased to `TaskTrackerSystem.Domain.Enums.TaskStatus` to avoid collision with `System.Threading.Tasks.TaskStatus`.
- AutoMapper is updated from 13.0.1 to 15.1.3, which contains the fix for GHSA-rvv3-g6hj-g44x.

After opening the solution in Visual Studio:
1. Build > Clean Solution
2. Build > Rebuild Solution
3. If needed, delete `bin` and `obj` folders and rebuild.
4. Then run the EF migration command.
