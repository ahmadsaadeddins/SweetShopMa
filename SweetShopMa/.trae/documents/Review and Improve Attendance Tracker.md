## Review Summary
- The attendance tracker spans `ViewModels/AdminViewModel.cs` (logic/UI state), `Models/AttendanceRecord.cs` (data model), `Views/AttendancePage.xaml(.cs)` (UI), and persistence in `Services/DatabaseService.cs`.
- Current design implements a custom work-cycle rule set (alternating 7/6 working-day cycles, reset days, month-specific adjustments) and computes daily pay from regular/overtime hours with user-specific multipliers.

## Strengths
- Clear MVVM separation: page binds to `AdminViewModel`; model is persisted via `DatabaseService`.
- Rich UI: employee picker, status/time inputs, live preview, metrics, calendar, and payroll PDF export (`Services/PdfService.cs`).
- Localization and RTL support (`LocalizationService` used in `AttendancePage.xaml.cs:39–58`).
- Safety checks: disallow future dates (`AdminViewModel.cs:966–969`); avoid duplicate records (`AdminViewModel.cs:972–980`).

## Issues and Risks
- Business rules coupling in VM:
  - Complex cycle logic (`AdminViewModel.cs:1453–1561` and helpers `WasResetDay`), interwoven with UI state; hard to test and maintain.
  - Special rules (31st day +8h OT; 28-day month +2 virtual days) are embedded in VM and not configurable (`AdminViewModel.cs:1849–1874`, `1341–1350`, `232–237`).
- Data constraints missing:
  - No unique index to enforce one record per user per date (currently only checked in code).
  - Potential query performance for attendance by user/date without an index.
- Time handling:
  - Uses local `Date + TimeSpan` composition; pay computations sensitive to DST/timezone without explicit normalization; partial risk if app time zone changes.
- Limited editability:
  - No edit/delete of existing attendance entries exposed in UI; corrections require manual DB work.
- Testability:
  - No unit tests around cycle/reset/overtime/pay rules; changes could regress silently.

## Recommendations
- Extract rule logic into a dedicated `AttendanceRulesService`:
  - Encapsulate: reset-day determination, cycle alternation (7/6), overtime computation, month-specific adjustments, and payroll math.
  - Make rules configurable (app settings/feature flags), not hard-coded.
  - Provide deterministic, testable methods.
- Strengthen data layer:
  - Add `CREATE UNIQUE INDEX IF NOT EXISTS idx_attendance_user_date ON AttendanceRecord(UserId, Date)`.
  - Add `CREATE INDEX IF NOT EXISTS idx_attendance_user_date_range ON AttendanceRecord(UserId, Date)` for faster range queries.
- Enhance UI/UX:
  - Add edit/delete flows for records; confirm dialogs; validation messaging (e.g., overlapping shifts).
  - Show pay breakdown (regular vs OT rates) per entry.
- Time/locale hygiene:
  - Normalize input times to a chosen timezone (or store UTC with offset) and render in local time consistently.
- Testing:
  - Add unit tests for: 7/6 cycle transitions, reset day detection, absence types, month-edge rules (28/31 days), payroll rounding.

## Proposed Implementation Steps
1. Create `Services/AttendanceRulesService` (no UI dependencies):
   - Methods: `IsResetDay(userId, date)`, `GetCycleInfo(userId, date)`, `CalculateDaily(AttendanceInput) -> AttendanceOutput` (regular/OT/pay, flags).
   - Inject into `AdminViewModel`; replace logic in `CalculateAttendanceForEntryAsync()` and related helpers with service calls.
2. Configuration:
   - Centralize rule parameters (cycle pattern, virtual days, 31st-day OT, work hours schedule) in a settings class.
3. Data layer improvements:
   - Add unique and range indexes in `DatabaseService` initialization for `AttendanceRecord` tables.
4. UI:
   - Add edit/delete commands and bindings in `AttendancePage.xaml` and `AdminViewModel`.
   - Add detailed pay breakdown labels.
5. Tests:
   - Create a test project or lightweight test harness for rule service; cover key edge cases.

## Verification Plan
- Populate sample users and records, exercise present/absent/reset flows and confirm:
  - Cycle alternation (7/6) behaves as intended across month boundaries.
  - Payroll totals match expectations in monthly summary (`AdminViewModel.cs:265–271`).
  - Calendar flags (present/absent/OT) render correctly in `AttendancePage.xaml:241–256`.
- Run performance checks on attendance queries with large data.

If you approve, I will implement the `AttendanceRulesService`, add indexes and UI edits, refactor the VM to use the service, and provide unit tests and a small verification dataset.