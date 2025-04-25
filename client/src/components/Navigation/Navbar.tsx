import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  Button,
  IconButton,
  Menu,
  MenuItem,
  Avatar,
  Box,
  useTheme,
  useMediaQuery
} from '@mui/material';
import { Menu as MenuIcon, AccountCircle } from '@mui/icons-material';
import { useAppAuth } from '../../App';
import { ru } from '../../locales/ru';

const Navbar: React.FC = () => {
  const { currentUser, logout } = useAppAuth();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('sm'));

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    handleClose();
    logout();
  };

  return (
    <AppBar position="static">
      <Toolbar>
        <Typography
          variant="h6"
          component={RouterLink}
          to="/"
          sx={{
            flexGrow: 1,
            textDecoration: 'none',
            color: 'inherit',
            display: 'flex',
            alignItems: 'center'
          }}
        >
          {ru.navbar.title}
        </Typography>

        {isMobile ? (
          <IconButton
            edge="start"
            color="inherit"
            aria-label="menu"
            onClick={handleMenu}
          >
            <MenuIcon />
          </IconButton>
        ) : (
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
            {currentUser ? (
              <>
                <Button
                  color="inherit"
                  component={RouterLink}
                  to="/dashboard"
                >
                  {ru.navbar.dashboard}
                </Button>
                <IconButton
                  onClick={handleMenu}
                  color="inherit"
                >
                  {currentUser.avatarUrl ? (
                    <Avatar
                      src={currentUser.avatarUrl}
                      alt={currentUser.firstName}
                      sx={{ width: 32, height: 32 }}
                    />
                  ) : (
                    <AccountCircle />
                  )}
                </IconButton>
              </>
            ) : (
              <>
                <Button
                  color="inherit"
                  component={RouterLink}
                  to="/login"
                >
                  {ru.navbar.login}
                </Button>
                <Button
                  color="inherit"
                  component={RouterLink}
                  to="/register"
                >
                  {ru.navbar.register}
                </Button>
              </>
            )}
          </Box>
        )}

        <Menu
          anchorEl={anchorEl}
          open={Boolean(anchorEl)}
          onClose={handleClose}
        >
          {currentUser ? (
            <>
              <MenuItem
                component={RouterLink}
                to="/profile"
                onClick={handleClose}
              >
                {ru.navbar.profile}
              </MenuItem>
              <MenuItem
                component={RouterLink}
                to="/settings"
                onClick={handleClose}
              >
                {ru.navbar.settings}
              </MenuItem>
              <MenuItem onClick={handleLogout}>
                {ru.navbar.logout}
              </MenuItem>
            </>
          ) : (
            <>
              <MenuItem
                component={RouterLink}
                to="/login"
                onClick={handleClose}
              >
                {ru.navbar.login}
              </MenuItem>
              <MenuItem
                component={RouterLink}
                to="/register"
                onClick={handleClose}
              >
                {ru.navbar.register}
              </MenuItem>
            </>
          )}
        </Menu>
      </Toolbar>
    </AppBar>
  );
};

export default Navbar; 