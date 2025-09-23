import { NgModule, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { VisitasComponent } from './components/visitas/visitas.component';
import { PersonasComponent } from './components/personas/personas.component';
import { LoginComponent } from './components/login/login.component';
import { ProductosComponent } from './components/productos/productos.component';
import { InformeTecnicoComponent } from './components/informe-tecnico/informe-tecnico.component';
import { ReunionesComponent } from './components/reuniones/reuniones.component';

import { AppRoutingModule } from './app-routing.module';

// Angular Material modules
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';

// Flatpickr module
import { FlatpickrModule } from 'angularx-flatpickr';
import { ServiceWorkerModule } from '@angular/service-worker';

import { environment } from '../environments/environment';


@NgModule({
  declarations: [
    AppComponent,
    VisitasComponent,
    PersonasComponent,
    LoginComponent,
    ProductosComponent,
    InformeTecnicoComponent,
    ReunionesComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    HttpClientModule,
    AppRoutingModule,

    // Angular Material
    MatPaginatorModule,
    MatTableModule,
    MatSelectModule,
    MatFormFieldModule,
    MatCheckboxModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,

    // Flatpickr
    FlatpickrModule.forRoot(),
      ServiceWorkerModule.register('ngsw-worker.js', {
        enabled: !isDevMode(),
        // Register the ServiceWorker as soon as the application is stable
        // or after 30 seconds (whichever comes first).
        registrationStrategy: 'registerWhenStable:30000'
      })  // 👈 NECESARIO para habilitar directiva mwlFlatpickr y opciones
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule {}
