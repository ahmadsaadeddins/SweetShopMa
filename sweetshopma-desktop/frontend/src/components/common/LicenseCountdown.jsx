import React, { useState, useEffect } from 'react';
import { Clock, AlertTriangle } from 'lucide-react';

/**
 * LicenseCountdown Component
 * Displays a countdown for trial licenses.
 */
const LicenseCountdown = () => {
    const [status, setStatus] = useState(null);
    const [timeLeft, setTimeLeft] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        fetchLicenseStatus();
    }, []);

    useEffect(() => {
        if (!status || !status.expiry_date) return;

        const timer = setInterval(() => {
            calculateTimeLeft(status.expiry_date);
        }, 1000);

        return () => clearInterval(timer);
    }, [status]);

    const fetchLicenseStatus = async () => {
        try {
            if (window.pywebview) {
                const result = await window.pywebview.api.get_license_status();
                setStatus(result);
                if (result.valid && result.expiry_date) {
                    calculateTimeLeft(result.expiry_date);
                }
            }
        } catch (error) {
            console.error("Failed to fetch license status:", error);
        } finally {
            setLoading(false);
        }
    };

    const calculateTimeLeft = (expiryDateStr) => {
        const expiryDate = new Date(expiryDateStr);
        const now = new Date();
        const difference = expiryDate - now;

        if (difference > 0) {
            const days = Math.floor(difference / (1000 * 60 * 60 * 24));
            const hours = Math.floor((difference / (1000 * 60 * 60)) % 24);
            const minutes = Math.floor((difference / 1000 / 60) % 60);
            const seconds = Math.floor((difference / 1000) % 60);

            setTimeLeft({ days, hours, minutes, seconds, total: difference });
        } else {
            setTimeLeft(null);
        }
    };

    if (loading || !status || !status.valid || !status.is_trial) {
        return null;
    }

    // Determine color based on time left
    const isUrgent = timeLeft && timeLeft.days < 3;
    const bgColor = isUrgent ? 'bg-red-100 text-red-800 border-red-200' : 'bg-amber-100 text-amber-800 border-amber-200';
    const iconColor = isUrgent ? 'text-red-500' : 'text-amber-500';

    return (
        <div className={`flex items-center gap-2 px-3 py-1.5 rounded-full border text-sm font-medium ${bgColor} transition-colors duration-300`}>
            {isUrgent ? <AlertTriangle className={`w-4 h-4 ${iconColor}`} /> : <Clock className={`w-4 h-4 ${iconColor}`} />}

            <span className="whitespace-nowrap">
                Trial:
                <span className="ml-1 font-bold font-mono">
                    {timeLeft ? (
                        <>
                            {timeLeft.days}d {timeLeft.hours}h {timeLeft.minutes}m
                        </>
                    ) : (
                        "Expired"
                    )}
                </span>
            </span>
        </div>
    );
};

export default LicenseCountdown;
