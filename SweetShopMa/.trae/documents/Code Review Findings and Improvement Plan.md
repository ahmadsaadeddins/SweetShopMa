## High-Impact Fixes
- Transactional checkout to ensure atomic order creation, stock updates, and cart clearing.
  - Add `ProcessCheckoutAsync` in `Services/DatabaseService.cs` to wrap checkout in a DB transaction.
  - Update `Services/CartService.cs:136` to call the new method instead of multi-step async writes.
- Faster product loading and sorting.
  - Replace in-memory sales aggregation with SQL `GROUP BY` in `Services/DatabaseService.cs:GetProductsAsync()` to avoid loading all `OrderItem`s.
- Stronger password hashing with backward compatibility.
  - Update `Utils/PasswordHelper.cs` to use 100,000 PBKDF2 iterations for new hashes and accept both formats during verification.

## Bugs / UX Issues
- Demo credentials mismatch:
  - `Views/LoginPage.xaml:105-111` shows `admin/admin123`, but seeded default user is `ama` with a different password (`Services/DatabaseService.cs:431-443`).
  - Plan: either update demo label to match seed or add seed users for the demo usernames.
- Minor code quality issue:
  - Duplicate `using System.Collections.Generic;` in `ViewModels/AdminViewModel.cs:2-3`.

## Performance & Reliability Improvements
- Add `RunInTransactionAsync(Action<SQLiteConnection>)` utility to `DatabaseService` for future atomic operations.
- Add SQLite indexes for common queries:
  - `OrderItem(ProductId)` and `Order(OrderDate)` via `PRAGMA` or `CREATE INDEX IF NOT EXISTS` after initialization.
- Reduce memory churn in reports:
  - Consider SQL aggregation in `ViewModels/AdminViewModel.cs:838-887` for top products instead of loading all items.

## UI/Usability Improvements
- Quick add error feedback:
  - In `ViewModels/ShopViewModel.cs:456-539`, currently focuses barcode silently on some errors. Add brief notifications using existing `NotificationMessage` to inform cashier (e.g., stock too low, invalid quantity).
- Localize platform service messages:
  - Move hardcoded error strings in `Platforms/Windows/WindowsPrintService.cs` and `Platforms/Windows/WindowsCashDrawerService.cs` to localization service for consistency.

## Security & Maintainability
- Mask or remove demo credentials from UI or generate them dynamically.
- Consider setting explicit SQLite open flags (ReadWrite|Create|SharedCache) when creating `SQLiteAsyncConnection` in `DatabaseService` for clarity.

## Concrete Changes To Apply
1. `Services/DatabaseService.cs`
   - Add `RunInTransactionAsync(Action<SQLiteConnection>)`.
   - Add `Order ProcessCheckoutAsync(Order order, List<CartItem> cartItems)` performing: insert order, insert order items, decrement stock, clear cart within a transaction.
   - Optimize `GetProductsAsync()` with SQL aggregation: `SELECT ProductId, SUM(Quantity) AS TotalSold FROM OrderItem GROUP BY ProductId`.
   - Optionally create indexes after initialization.
2. `Services/CartService.cs`
   - Replace multi-step checkout with a single call to `ProcessCheckoutAsync` and clear in-memory cart.
3. `Utils/PasswordHelper.cs`
   - Hash format: `iterations:salt:hash` with 100,000 iterations for new passwords.
   - Verification: support both legacy `salt:hash` (10,000) and new format.
4. `Views/LoginPage.xaml`
   - Update demo credentials text to match actual seed or remove it.
5. `ViewModels/AdminViewModel.cs`
   - Remove duplicate `using` line.
6. Optional: Localize strings in Windows services.

## Verification Plan
- Run app and perform end-to-end checkout:
  - Add items, checkout, verify order is saved, stock reduced, cart cleared; check behavior on simulated failure (transaction integrity).
- Login with existing seed and a newly created user to confirm password verification supports both formats.
- Validate product list loads quickly with many orders.
- Confirm UI feedback shows notifications on invalid quick add.

Please confirm, and I will implement these changes, run through verification, and share the results. 