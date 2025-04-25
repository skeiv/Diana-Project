import React from 'react';
import { Link as RouterLink } from 'react-router-dom';
import {
  Container,
  Typography,
  Grid,
  Card,
  CardContent,
  CardActions,
  Button,
  Box,
  Paper
} from '@mui/material';
import {
  Work as WorkIcon,
  School as SchoolIcon,
  Person as PersonIcon
} from '@mui/icons-material';

const Home: React.FC = () => {
  return (
    <Container maxWidth="lg">
      <Box
        sx={{
          pt: 8,
          pb: 6,
          textAlign: 'center'
        }}
      >
        <Typography
          component="h1"
          variant="h2"
          color="text.primary"
          gutterBottom
        >
          Добро пожаловать в JobSearchApp
        </Typography>
        <Typography variant="h5" color="text.secondary" paragraph>
          Найдите работу своей мечты или найдите идеального сотрудника
        </Typography>
      </Box>

      <Grid container spacing={4} sx={{ mb: 8 }}>
        <Grid item xs={12} md={4}>
          <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <CardContent sx={{ flexGrow: 1 }}>
              <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
                <PersonIcon sx={{ fontSize: 40, color: 'primary.main' }} />
              </Box>
              <Typography gutterBottom variant="h5" component="h2" align="center">
                Для соискателей
              </Typography>
              <Typography>
                <ul style={{ listStyle: 'none', padding: 0 }}>
                  <li>✓ Создайте резюме</li>
                  <li>✓ Найдите подходящие вакансии</li>
                  <li>✓ Отслеживайте отклики</li>
                  <li>✓ Проходите курсы для повышения квалификации</li>
                </ul>
              </Typography>
            </CardContent>
            <CardActions>
              <Button
                fullWidth
                variant="contained"
                component={RouterLink}
                to="/register"
                sx={{ mt: 2 }}
              >
                Зарегистрироваться как соискатель
              </Button>
            </CardActions>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <CardContent sx={{ flexGrow: 1 }}>
              <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
                <WorkIcon sx={{ fontSize: 40, color: 'primary.main' }} />
              </Box>
              <Typography gutterBottom variant="h5" component="h2" align="center">
                Для работодателей
              </Typography>
              <Typography>
                <ul style={{ listStyle: 'none', padding: 0 }}>
                  <li>✓ Размещайте вакансии</li>
                  <li>✓ Найдите лучших кандидатов</li>
                  <li>✓ Управляйте откликами</li>
                  <li>✓ Создавайте обучающие курсы</li>
                </ul>
              </Typography>
            </CardContent>
            <CardActions>
              <Button
                fullWidth
                variant="contained"
                component={RouterLink}
                to="/register"
                sx={{ mt: 2 }}
              >
                Зарегистрироваться как работодатель
              </Button>
            </CardActions>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
            <CardContent sx={{ flexGrow: 1 }}>
              <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
                <SchoolIcon sx={{ fontSize: 40, color: 'primary.main' }} />
              </Box>
              <Typography gutterBottom variant="h5" component="h2" align="center">
                Для преподавателей
              </Typography>
              <Typography>
                <ul style={{ listStyle: 'none', padding: 0 }}>
                  <li>✓ Создавайте курсы</li>
                  <li>✓ Обучайте студентов</li>
                  <li>✓ Отслеживайте прогресс</li>
                  <li>✓ Получайте отзывы</li>
                </ul>
              </Typography>
            </CardContent>
            <CardActions>
              <Button
                fullWidth
                variant="contained"
                component={RouterLink}
                to="/register"
                sx={{ mt: 2 }}
              >
                Зарегистрироваться как преподаватель
              </Button>
            </CardActions>
          </Card>
        </Grid>
      </Grid>

      <Paper
        sx={{
          p: 4,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          mb: 8
        }}
      >
        <Grid container spacing={4} justifyContent="center">
          <Grid item xs={12} sm={4}>
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h3" color="primary" gutterBottom>
                1000+
              </Typography>
              <Typography variant="h6" color="text.secondary">
                Активных вакансий
              </Typography>
            </Box>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h3" color="primary" gutterBottom>
                500+
              </Typography>
              <Typography variant="h6" color="text.secondary">
                Курсов
              </Typography>
            </Box>
          </Grid>
          <Grid item xs={12} sm={4}>
            <Box sx={{ textAlign: 'center' }}>
              <Typography variant="h3" color="primary" gutterBottom>
                10000+
              </Typography>
              <Typography variant="h6" color="text.secondary">
                Пользователей
              </Typography>
            </Box>
          </Grid>
        </Grid>
      </Paper>
    </Container>
  );
};

export default Home; 