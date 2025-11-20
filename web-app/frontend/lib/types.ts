export interface User {
  id: number
  username: string
  name: string
  role: 'Developer' | 'Admin' | 'Moderator' | 'User'
  monthly_salary: string
  is_enabled: boolean
  created_date: string
  is_developer: boolean
  is_admin: boolean
  is_moderator: boolean
  is_user: boolean
  can_manage_users: boolean
  can_manage_stock: boolean
  can_use_attendance_tracker: boolean
  can_restock: boolean
}

export interface Product {
  id: number
  name: string
  emoji: string
  barcode: string
  price: string
  stock: string
  is_sold_by_weight: boolean
  unit_label: string
}

export interface CartItem {
  id: number
  product: Product
  product_id: number
  quantity: string
  item_total: string
}

export interface Order {
  id: number
  user_id: number
  user_name: string
  order_date: string
  total: string
  item_count: number
  status: string
  items: OrderItem[]
}

export interface OrderItem {
  id: number
  product_id: number
  product_name: string
  product_emoji: string
  price: string
  quantity: string
  is_sold_by_weight: boolean
  item_total: string
  unit_label: string
}

export interface AttendanceRecord {
  id: number
  user_id: number
  user_name: string
  date: string
  status: string
  is_present: boolean
  regular_hours: string
  overtime_hours: string
  daily_pay: string
  total_hours: string
  check_in_time: string | null
  check_out_time: string | null
  notes: string
}

export interface RestockRecord {
  id: number
  product_id: number
  product_name: string
  product_emoji: string
  quantity_added: string
  stock_before: string
  stock_after: string
  user_id: number
  user_name: string
  restock_date: string
}

export interface Reports {
  total_sales: string
  total_orders: number
  average_order_value: string
  total_items_sold: string
  last_7_days_sales: string
  top_products: Array<{
    product_name: string
    product_emoji: string
    total_sold: string
  }>
}

