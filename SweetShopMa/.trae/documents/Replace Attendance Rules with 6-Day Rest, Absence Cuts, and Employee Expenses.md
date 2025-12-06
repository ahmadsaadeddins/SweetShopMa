## Changes To Implement
- Remove old 7→6 reset-cycle logic entirely.
- New monthly rules:
  - Earn 1 paid rest day for every 6 worked days (non-consecutive allowed).
  - Rest day is paid at normal daily rate: salary/30.
  - Example: 24 worked days → 4 rest days; 31 worked days → 5 rest days.
  - Absence with permission: deduct 1 day.
  - Absence without permission: deduct 2 days; track counts for reporting.
- Add employee expenses tracking (per employee: amount, category, notes, date).

## Implementation
- Create `Models/EmployeeExpense.cs` and add CRUD in `DatabaseService`.
- Update `AttendanceRulesService` to compute monthly summary per new rules (no reset detection, no 31st OT logic).
- Refactor `AdminViewModel` monthly summary to use new rules: compute WorkedDays, EarnedRestDays, RestDayPayout, Absence counts, AbsenceDeductions, TotalPayroll.
- Extend `MonthlyAttendanceSummary` to include new fields.
- Update `Views/AttendancePage.xaml` summary/list bindings to display new fields (worked days, rest days, rest payout, absence deductions).

## Verification
- Seed sample records and verify: 24 presents → 4 rest days paid; 31 presents → 5 rest days.
- Check monthly totals include rest day payout and absence deductions.
- Add a sample employee expense and verify it appears.
