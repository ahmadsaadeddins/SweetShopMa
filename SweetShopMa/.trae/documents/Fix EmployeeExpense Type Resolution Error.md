## Issue
- Build error CS0246 in `Services/PdfService.cs`: `EmployeeExpense` not found.
- Cause: Missing namespace import for models in `PdfService.cs`.

## Fix Plan
- Add `using SweetShopMa.Models;` at the top of `Services/PdfService.cs`.
- Alternatively, fully-qualify the generic type in the method signature: `List<SweetShopMa.Models.EmployeeExpense>` (not required if using is added).

## Verification
- Rebuild to confirm CS0246 is resolved.
- Generate an employee payroll PDF to ensure `GenerateEmployeePayrollPdfAsync(...)` compiles and runs.