/**
 * Validation utility functions
 */

/**
 * Validate email address
 */
export function isValidEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

/**
 * Validate phone number (Egyptian format)
 */
export function isValidPhone(phone) {
    // Egyptian phone: 01xxxxxxxxx (11 digits starting with 01)
    const re = /^01[0-9]{9}$/;
    return re.test(phone);
}

/**
 * Validate required field
 */
export function isRequired(value) {
    if (value === null || value === undefined) {
        return false;
    }

    if (typeof value === 'string') {
        return value.trim().length > 0;
    }

    if (Array.isArray(value)) {
        return value.length > 0;
    }

    return true;
}

/**
 * Validate numeric value
 */
export function isNumeric(value) {
    if (value === null || value === undefined || value === '') {
        return false;
    }

    return !isNaN(value) && !isNaN(parseFloat(value));
}

/**
 * Validate positive number
 */
export function isPositive(value) {
    return isNumeric(value) && parseFloat(value) > 0;
}

/**
 * Validate non-negative number
 */
export function isNonNegative(value) {
    return isNumeric(value) && parseFloat(value) >= 0;
}

/**
 * Validate minimum length
 */
export function minLength(value, min) {
    if (!value) return false;
    return value.length >= min;
}

/**
 * Validate maximum length
 */
export function maxLength(value, max) {
    if (!value) return true;
    return value.length <= max;
}

/**
 * Validate range
 */
export function isInRange(value, min, max) {
    if (!isNumeric(value)) return false;
    const num = parseFloat(value);
    return num >= min && num <= max;
}

/**
 * Validate barcode format
 */
export function isValidBarcode(barcode) {
    if (!barcode) return false;
    // Barcode should be alphanumeric, 8-20 characters
    const re = /^[A-Z0-9]{8,20}$/i;
    return re.test(barcode);
}

/**
 * Validate SKU format
 */
export function isValidSKU(sku) {
    if (!sku) return false;
    // SKU should be alphanumeric with hyphens/underscores, 3-30 characters
    const re = /^[A-Z0-9_-]{3,30}$/i;
    return re.test(sku);
}

/**
 * Validate product name
 */
export function isValidProductName(name) {
    if (!name) return false;
    // Name should be 2-200 characters
    return name.trim().length >= 2 && name.trim().length <= 200;
}

/**
 * Validate price
 */
export function isValidPrice(price) {
    return isNonNegative(price) && parseFloat(price) <= 999999.99;
}

/**
 * Validate quantity
 */
export function isValidQuantity(quantity) {
    return isNonNegative(quantity) && parseFloat(quantity) <= 999999;
}

/**
 * Validate discount percentage
 */
export function isValidDiscount(discount) {
    return isNonNegative(discount) && parseFloat(discount) <= 100;
}

/**
 * Common form validator
 */
export function validateForm(formData, rules) {
    const errors = {};

    for (const field in rules) {
        const value = formData[field];
        const fieldRules = rules[field];

        for (const rule of fieldRules) {
            if (rule.required && !isRequired(value)) {
                errors[field] = rule.message || `${field} is required`;
                break;
            }

            if (rule.email && value && !isValidEmail(value)) {
                errors[field] = rule.message || 'Invalid email format';
                break;
            }

            if (rule.phone && value && !isValidPhone(value)) {
                errors[field] = rule.message || 'Invalid phone number';
                break;
            }

            if (rule.numeric && value && !isNumeric(value)) {
                errors[field] = rule.message || 'Must be a number';
                break;
            }

            if (rule.positive && value && !isPositive(value)) {
                errors[field] = rule.message || 'Must be a positive number';
                break;
            }

            if (rule.min && value && !minLength(value, rule.min)) {
                errors[field] = rule.message || `Must be at least ${rule.min} characters`;
                break;
            }

            if (rule.max && value && !maxLength(value, rule.max)) {
                errors[field] = rule.message || `Must be no more than ${rule.max} characters`;
                break;
            }

            if (rule.range && value && !isInRange(value, rule.range[0], rule.range[1])) {
                errors[field] = rule.message || `Must be between ${rule.range[0]} and ${rule.range[1]}`;
                break;
            }
        }
    }

    return {
        isValid: Object.keys(errors).length === 0,
        errors
    };
}
