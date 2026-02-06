"""
Cash Drawer Service for SweetShopMa Desktop Application.

Opens cash drawer via ESC/POS commands to thermal printer.
"""

import subprocess
import platform
import os
import logging

logger = logging.getLogger(__name__)


class CashDrawerService:
    """Opens cash drawer via ESC/POS commands to printer"""
    
    # ESC/POS command to open cash drawer
    # ESC p m t1 t2
    # ESC = 0x1B, p = 0x70, m = 0x00 (pin 0), t1 = 0x19 (25ms), t2 = 0xFA (250ms)
    DRAWER_COMMAND = bytes([0x1B, 0x70, 0x00, 0x19, 0xFA])
    
    def open_drawer(self):
        """
        Open cash drawer by sending ESC/POS command to printer.
        
        Returns:
            dict: Success status and error message if failed
        """
        logger.info('Cash drawer open requested')
        
        try:
            # Check platform support
            if platform.system() not in ['Windows', 'Linux', 'Darwin']:
                logger.warning(f'Unsupported platform for cash drawer: {platform.system()}')
                return {'success': False, 'error': 'Unsupported platform'}
            
            if platform.system() == 'Windows':
                result = self._open_drawer_windows()
            else:
                result = self._open_drawer_generic()
            
            if result:
                logger.info('Cash drawer opened successfully')
                return {'success': True}
            else:
                logger.warning('Failed to open cash drawer: Command returned false')
                return {'success': False, 'error': 'Failed to send drawer command'}
                
        except subprocess.TimeoutExpired:
            logger.error('Cash drawer timeout: Printer communication timed out')
            return {'success': False, 'error': 'Printer communication timeout'}
        except PermissionError:
            logger.error('Cash drawer permission denied: Insufficient privileges')
            return {'success': False, 'error': 'Permission denied - check printer access'}
        except Exception as e:
            logger.exception(f'Unexpected error opening cash drawer: {str(e)}')
            return {'success': False, 'error': f'Unexpected error: {str(e)}'}
    
    def _open_drawer_windows(self):
        """
        Windows: Use copy command to send raw bytes to printer.
        
        Returns:
            bool: True if successful
        """
        try:
            # Get default printer name
            result = subprocess.run(
                ['powershell', '-Command', 
                 'Get-WmiObject -Query "SELECT * FROM Win32_Printer WHERE Default=\\"TRUE\\"" | Select-Object -ExpandProperty Name'],
                capture_output=True,
                text=True,
                timeout=5
            )
            
            if result.returncode != 0:
                raise Exception(f'PowerShell error: {result.stderr}')
            
            if not result.stdout.strip():
                logger.warning('No default printer configured')
                return False
            
            printer_name = result.stdout.strip()
            
            # Create temp file with drawer command
            temp_file = tempfile.NamedTemporaryFile(delete=False, suffix='.bin')
            temp_file.write(self.DRAWER_COMMAND)
            temp_file.close()
            
            try:
                # Send to printer using copy command
                subprocess.run(
                    ['cmd', '/c', 'copy', '/b', temp_file.name, f'"{printer_name}"'],
                    capture_output=True,
                    timeout=5,
                    creationflags=subprocess.CREATE_NO_WINDOW
                )
                return True
            except subprocess.CalledProcessError as e:
                raise Exception(f'Copy command failed: {e.stderr}')
            finally:
                try:
                    os.unlink(temp_file.name)
                except:
                    pass
                    
        except subprocess.TimeoutExpired:
            raise Exception('PowerShell command timed out')
        except Exception as e:
            logger.error(f'Error opening cash drawer on Windows: {str(e)}')
            raise
    
    def _open_drawer_generic(self):
        """
        Generic: Try lpr command (Linux/macOS).
        
        Returns:
            bool: True if successful
        """
        try:
            subprocess.run(
                ['lpr', '-P', 'default'],
                input=self.DRAWER_COMMAND,
                timeout=5
            )
            return True
        except:
            return False


# Import tempfile for Windows method
import tempfile
