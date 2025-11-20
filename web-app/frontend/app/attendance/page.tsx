'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth-context'
import { useLocalization } from '@/lib/localization-context'
import api from '@/lib/api'
import { User, AttendanceRecord } from '@/lib/types'
import { showToast } from '@/lib/toast'

export default function AttendancePage() {
  const router = useRouter()
  const { user, logout } = useAuth()
  const { t } = useLocalization()
  const [users, setUsers] = useState<User[]>([])
  const [selectedUserId, setSelectedUserId] = useState<number | null>(null)
  const [date, setDate] = useState(new Date().toISOString().split('T')[0])
  const [status, setStatus] = useState<'Present' | 'Absent'>('Present')
  const [regularHours, setRegularHours] = useState('8')
  const [overtimeHours, setOvertimeHours] = useState('0')
  const [checkInTime, setCheckInTime] = useState('08:00')
  const [checkOutTime, setCheckOutTime] = useState('16:00')

  // Calculate hours from check-in and check-out times
  const calculateHoursFromTimes = (checkIn: string, checkOut: string) => {
    if (!checkIn || !checkOut) {
      setRegularHours('8')
      setOvertimeHours('0')
      return
    }

    const [inHour, inMinute] = checkIn.split(':').map(Number)
    const [outHour, outMinute] = checkOut.split(':').map(Number)

    const checkInMinutes = inHour * 60 + inMinute
    const checkOutMinutes = outHour * 60 + outMinute

    // Handle case where check-out is next day (e.g., night shift)
    let totalMinutes = checkOutMinutes - checkInMinutes
    if (totalMinutes < 0) {
      totalMinutes += 24 * 60 // Add 24 hours
    }

    const totalHours = totalMinutes / 60
    const standardHours = 8

    if (totalHours <= standardHours) {
      setRegularHours(totalHours.toFixed(2))
      setOvertimeHours('0')
    } else {
      setRegularHours(standardHours.toFixed(2))
      setOvertimeHours((totalHours - standardHours).toFixed(2))
    }
  }

  // Update hours when check-in or check-out time changes
  useEffect(() => {
    if (status === 'Present' && checkInTime && checkOutTime) {
      calculateHoursFromTimes(checkInTime, checkOutTime)
    } else if (status === 'Absent') {
      setRegularHours('0')
      setOvertimeHours('0')
    }
  }, [checkInTime, checkOutTime, status])

  const [notes, setNotes] = useState('')
  const [records, setRecords] = useState<AttendanceRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!user || !user.can_use_attendance_tracker) {
      router.push('/shop')
      return
    }
    loadData()
  }, [user, router])

  const loadData = async () => {
    try {
      const [usersRes, recordsRes] = await Promise.all([
        api.get('/users/'),
        api.get('/attendance/'),
      ])
      setUsers(usersRes.data.results || usersRes.data)
      setRecords(recordsRes.data.results || recordsRes.data)
      if (usersRes.data.results?.length > 0 || usersRes.data.length > 0) {
        const firstUser = usersRes.data.results?.[0] || usersRes.data[0]
        setSelectedUserId(firstUser.id)
      }
    } catch (err) {
      console.error('Failed to load data:', err)
    } finally {
      setLoading(false)
    }
  }

  const calculateDailyPay = () => {
    if (!selectedUserId) return 0
    const selectedUser = users.find(u => u.id === selectedUserId)
    if (!selectedUser) return 0
    
    const monthlySalary = parseFloat(selectedUser.monthly_salary)
    const dailyRate = monthlySalary / 30
    const regularPay = (parseFloat(regularHours) * dailyRate) / 8
    const overtimePay = parseFloat(overtimeHours) * (dailyRate / 8) * 1.5
    
    return regularPay + overtimePay
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!selectedUserId) {
      alert('Please select an employee')
      return
    }

    setSubmitting(true)
    try {
      const selectedUser = users.find(u => u.id === selectedUserId)
      const dailyPay = calculateDailyPay()
      
      await api.post('/attendance/', {
        user_id: selectedUserId,
        user_name: selectedUser?.name || selectedUser?.username,
        date,
        status,
        is_present: status === 'Present',
        regular_hours: parseFloat(regularHours),
        overtime_hours: parseFloat(overtimeHours),
        daily_pay: dailyPay,
        check_in_time: status === 'Present' ? checkInTime : null,
        check_out_time: status === 'Present' ? checkOutTime : null,
        notes,
      })
      
      showToast('Attendance recorded successfully!', 'success')
      setDate(new Date().toISOString().split('T')[0])
      setStatus('Present')
      setCheckInTime('08:00')
      setCheckOutTime('16:00')
      // Hours will be recalculated automatically by useEffect
      setNotes('')
      await loadData()
    } catch (err: any) {
      // Only log unexpected errors (409 conflicts are expected for duplicates)
      if (err.response?.status !== 409) {
        console.error('Attendance error:', err)
        console.error('Error response:', err.response)
        console.error('Error response status:', err.response?.status)
        console.error('Error data:', err.response?.data)
      }
      
      // Try multiple ways to extract error message
      let errorMessage = t('FailedToRecordAttendance')
      
      // Check if response exists
      if (err.response) {
        const responseData = err.response.data
        
        // If data is empty object, check if it's actually empty or just not parsed
        if (responseData && typeof responseData === 'object' && Object.keys(responseData).length === 0) {
          // Empty object - use status-based message
          if (err.response.status === 400) {
            errorMessage = 'Bad request - please check your input'
          } else if (err.response.status === 409) {
            errorMessage = t('AttendanceRecordExists')
          } else if (err.response.status === 500) {
            errorMessage = 'Server error occurred'
          } else {
            errorMessage = `Request failed with status ${err.response.status}`
          }
        } else if (responseData) {
          // Try to extract error message
          let rawMessage = ''
          if (typeof responseData === 'string') {
            rawMessage = responseData
          } else if (responseData.error) {
            rawMessage = responseData.error
          } else if (responseData.detail) {
            rawMessage = responseData.detail
          } else if (responseData.message) {
            rawMessage = responseData.message
          } else if (Array.isArray(responseData) && responseData.length > 0) {
            rawMessage = responseData[0]
          } else if (typeof responseData === 'object') {
            // Try to get first value from object
            const firstKey = Object.keys(responseData)[0]
            if (firstKey) {
              const firstValue = responseData[firstKey]
              if (Array.isArray(firstValue) && firstValue.length > 0) {
                rawMessage = firstValue[0]
              } else if (typeof firstValue === 'string') {
                rawMessage = firstValue
              }
            }
          }
          
          // Check if it's a duplicate attendance error and localize it
          if (rawMessage && rawMessage.includes('Attendance record already exists')) {
            // Extract the date from the message if present
            const dateMatch = rawMessage.match(/(\d{4}-\d{2}-\d{2})/)
            if (dateMatch) {
              errorMessage = `${t('AttendanceRecordExists')} ${dateMatch[1]}`
            } else {
              errorMessage = t('AttendanceRecordExists')
            }
          } else {
            errorMessage = rawMessage
          }
        } else if (err.response.status === 409 || err.response.status === 400) {
          errorMessage = t('AttendanceRecordExists')
        }
      } else if (err.message) {
        errorMessage = err.message
      }
      
      console.log('Final error message:', errorMessage)
      console.log('Showing toast with message:', errorMessage)
      showToast(errorMessage, 'error')
    } finally {
      setSubmitting(false)
    }
  }

  if (loading) {
    return <div className="flex items-center justify-center min-h-screen">Loading...</div>
  }

  const dailyPay = calculateDailyPay()

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-6xl mx-auto">
        {/* Header */}
        <div className="bg-primary text-white p-6 rounded-lg mb-4 flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold">{t('AttendanceTracker')}</h1>
            <p className="text-sm opacity-90">{user?.name || user?.username}</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => router.push('/shop')}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('BackToShop')}
            </button>
            <button
              onClick={logout}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('Logout')}
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {/* Form */}
          <div className="bg-white p-6 rounded-lg shadow">
            <h2 className="text-2xl font-bold mb-4">{t('RecordAttendance')}</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
              <div>
                <label className="block text-sm font-medium mb-1">{t('Employee')}</label>
                <select
                  value={selectedUserId || ''}
                  onChange={(e) => setSelectedUserId(Number(e.target.value))}
                  required
                  className="w-full px-3 py-2 border rounded-md"
                >
                  <option value="">Select employee</option>
                  {users.map((u) => (
                    <option key={u.id} value={u.id}>
                      {u.name} ({u.username}) - {u.role}
                    </option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">{t('Date')}</label>
                <input
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                  required
                  className="w-full px-3 py-2 border rounded-md"
                />
              </div>

              <div>
                <label className="block text-sm font-medium mb-1">{t('Status')}</label>
                <select
                  value={status}
                  onChange={(e) => setStatus(e.target.value as 'Present' | 'Absent')}
                  required
                  className="w-full px-3 py-2 border rounded-md"
                >
                  <option value="Present">{t('Present')}</option>
                  <option value="Absent">{t('Absent')}</option>
                </select>
              </div>

              {status === 'Present' && (
                <>
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">{t('CheckIn')}</label>
                      <input
                        type="time"
                        value={checkInTime}
                        onChange={(e) => setCheckInTime(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md"
                        required
                      />
                    </div>
                    <div>
                      <label className="block text-sm font-medium mb-1">{t('CheckOut')}</label>
                      <input
                        type="time"
                        value={checkOutTime}
                        onChange={(e) => setCheckOutTime(e.target.value)}
                        className="w-full px-3 py-2 border rounded-md"
                        required
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-sm font-medium mb-1">{t('RegularHours')}</label>
                      <input
                        type="number"
                        value={regularHours}
                        readOnly
                        className="w-full px-3 py-2 border rounded-md bg-gray-100 cursor-not-allowed"
                      />
                      <p className="text-xs text-gray-500 mt-1">{t('CalculatedAutomatically')}</p>
                    </div>
                    <div>
                      <label className="block text-sm font-medium mb-1">{t('OvertimeHours')}</label>
                      <input
                        type="number"
                        value={overtimeHours}
                        readOnly
                        className="w-full px-3 py-2 border rounded-md bg-gray-100 cursor-not-allowed"
                      />
                      <p className="text-xs text-gray-500 mt-1">{t('CalculatedAutomatically')}</p>
                    </div>
                  </div>

                  {selectedUserId && (
                    <div className="p-3 bg-blue-50 rounded">
                      <div className="text-sm text-gray-600">{t('DailyPay')}</div>
                      <div className="text-lg font-bold">${dailyPay.toFixed(2)}</div>
                      <div className="text-xs text-gray-500">
                        {t('RegularHours')}: {regularHours}h • {t('OvertimeHours')}: {overtimeHours}h
                      </div>
                    </div>
                  )}
                </>
              )}

              <div>
                <label className="block text-sm font-medium mb-1">{t('NotesOptional')}</label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  rows={3}
                  className="w-full px-3 py-2 border rounded-md"
                  placeholder={t('Notes')}
                />
              </div>

              {/* Toast container positioned above the submit button */}
              <div id="attendance-toast-container" className="min-h-[80px]"></div>

              <button
                type="submit"
                disabled={submitting}
                className="w-full bg-primary text-white py-2 px-4 rounded-md hover:bg-primary/90 disabled:opacity-50"
              >
                {submitting ? t('Loading') : t('RecordAttendance')}
              </button>
            </form>
          </div>

          {/* Recent Records */}
          <div className="bg-white p-6 rounded-lg shadow">
            <h2 className="text-2xl font-bold mb-4">{t('RecentRecords')}</h2>
            <div className="space-y-2 max-h-96 overflow-y-auto">
              {records.slice(0, 20).map((record) => (
                <div key={record.id} className="p-3 border rounded">
                  <div className="flex justify-between items-start">
                    <div>
                      <div className="font-semibold">{record.user_name}</div>
                      <div className="text-sm text-gray-600">
                        {new Date(record.date).toLocaleDateString()} - {record.status === 'Present' ? t('Present') : t('Absent')}
                      </div>
                      {record.is_present && (
                        <div className="text-xs text-gray-500">
                          {record.regular_hours}h regular + {record.overtime_hours}h OT = {parseFloat(record.total_hours).toFixed(1)}h total
                        </div>
                      )}
                    </div>
                    {record.is_present && (
                      <div className="text-right">
                        <div className="font-bold">${parseFloat(record.daily_pay).toFixed(2)}</div>
                      </div>
                    )}
                  </div>
                </div>
              ))}
              {records.length === 0 && (
                <div className="text-center text-gray-500 py-8">{t('NoRecords')}</div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}


