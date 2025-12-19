import { ref } from 'vue'

// Shared customer state
const selectedCustomerId = ref('CUST001')

export function useCustomer() {
  const setCustomerId = (id: string) => {
    selectedCustomerId.value = id
  }

  return {
    customerId: selectedCustomerId,
    setCustomerId,
  }
}
